using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sanalia.Api.Contracts;
using Sanalia.Core.Entities;
using Sanalia.Core.Enums;
using Sanalia.Core.Interfaces;
using Sanalia.Infrastructure.Data.Context;

namespace Sanalia.Tests;

public sealed class ApiIntegrationTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Client_registration_authentication_and_catalog_use_expected_contracts()
    {
        using var client = factory.CreateClient();

        var unauthorized = await client.GetAsync("/api/solicitudes-asesoria/mias");
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorized.StatusCode);
        Assert.Equal("application/problem+json", unauthorized.Content.Headers.ContentType?.MediaType);

        await SeedCatalogAsync();

        var registration = await client.PostAsJsonAsync("/api/auth/register", new
        {
            nombre = "Cliente de prueba",
            documentoIdentidad = "TEST-001",
            telefono = "555-0100",
            correo = "cliente@example.com",
            contraseña = "Secret123!"
        });
        Assert.Equal(HttpStatusCode.Created, registration.StatusCode);
        Assert.Equal("/api/auth/me", registration.Headers.Location?.OriginalString);
        var registered = await registration.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(registered);
        Assert.False(registered.EsAsesor);
        Assert.False(string.IsNullOrWhiteSpace(registered.AccessToken));

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", registered.AccessToken);

        var currentUser = await client.GetFromJsonAsync<CurrentUserResponse>("/api/auth/me");
        Assert.NotNull(currentUser);
        Assert.Equal("cliente@example.com", currentUser.Correo);

        var forbiddenCustomerDirectory = await client.GetAsync("/api/clientes");
        Assert.Equal(HttpStatusCode.Forbidden, forbiddenCustomerDirectory.StatusCode);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SanaliaDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            db.Asesores.AddRange(
                new Asesor
                {
                    Nombre = "Asesora de prueba",
                    Correo = "asesora@example.com",
                    Contraseña = hasher.Hash("AdvisorPass123!"),
                    Rol = RolUsuario.Asesor
                },
                new Asesor
                {
                    Nombre = "Administrador de prueba",
                    Correo = "admin@example.com",
                    Contraseña = hasher.Hash("AdminPass123!"),
                    Rol = RolUsuario.Administrador
                });
            await db.SaveChangesAsync();
        }

        var advisorLogin = await client.PostAsJsonAsync("/api/auth/login/asesor", new
        {
            correo = "asesora@example.com",
            contraseña = "AdvisorPass123!"
        });
        Assert.Equal(HttpStatusCode.OK, advisorLogin.StatusCode);
        using var advisorJson = JsonDocument.Parse(await advisorLogin.Content.ReadAsStringAsync());
        Assert.Equal("Asesor", advisorJson.RootElement.GetProperty("rol").GetString());
        var advisorToken = advisorJson.RootElement.GetProperty("accessToken").GetString();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                advisorToken);

        var customerDirectory = await client.GetFromJsonAsync<List<ClienteResponse>>(
            "/api/clientes?busqueda=Cliente%20de%20prueba");
        Assert.NotNull(customerDirectory);
        var listedCustomer = Assert.Single(customerDirectory);
        Assert.Equal("Cliente de prueba", listedCustomer.Nombre);
        var advisorInsurers = await client.GetAsync("/api/aseguradoras");
        Assert.Equal(HttpStatusCode.Forbidden, advisorInsurers.StatusCode);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", registered.AccessToken);

        var missingRequest = await client.GetAsync("/api/solicitudes-asesoria/999");
        Assert.Equal(HttpStatusCode.NotFound, missingRequest.StatusCode);
        Assert.Equal("application/problem+json", missingRequest.Content.Headers.ContentType?.MediaType);

        var forbidden = await client.PostAsJsonAsync("/api/horarios", new
        {
            fecha = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)),
            horaInicio = "09:00:00",
            horaFin = "10:00:00"
        });
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);

        var products = await client.GetAsync("/api/catalog/tipos-seguro/1/productos");
        Assert.Equal(HttpStatusCode.OK, products.StatusCode);
        using var json = JsonDocument.Parse(await products.Content.ReadAsStringAsync());
        Assert.Equal("Completo", json.RootElement[0].GetProperty("nivelCobertura").GetString());

        var login = await client.PostAsJsonAsync("/api/auth/login/cliente", new
        {
            correo = "cliente@example.com",
            contraseña = "Secret123!"
        });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        var assistance = await client.PostAsJsonAsync("/api/solicitudes-asistencia", new
        {
            motivo = "Necesito una explicación de mi cobertura"
        });
        Assert.Equal(HttpStatusCode.Created, assistance.StatusCode);
        using var assistanceJson = JsonDocument.Parse(await assistance.Content.ReadAsStringAsync());
        var assistanceId = assistanceJson.RootElement.GetProperty("id").GetInt32();
        Assert.Equal("Pendiente", assistanceJson.RootElement.GetProperty("estado").GetString());

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", advisorToken);
        var staffAssistanceResponse = await client.GetAsync("/api/solicitudes-asistencia/personal");
        Assert.Equal(HttpStatusCode.OK, staffAssistanceResponse.StatusCode);
        using var staffAssistance = JsonDocument.Parse(await staffAssistanceResponse.Content.ReadAsStringAsync());
        Assert.Contains(staffAssistance.RootElement.EnumerateArray(), request =>
            request.GetProperty("id").GetInt32() == assistanceId);
        var assignedAssistance = await client.PostAsync(
            $"/api/solicitudes-asistencia/{assistanceId}/asignacion", content: null);
        Assert.Equal(HttpStatusCode.OK, assignedAssistance.StatusCode);
        var resolvedAssistance = await client.PatchAsJsonAsync(
            $"/api/solicitudes-asistencia/{assistanceId}/estado",
            new { estado = "Resuelta" });
        Assert.Equal(HttpStatusCode.OK, resolvedAssistance.StatusCode);
        var allStaffAssistanceResponse = await client.GetAsync("/api/solicitudes-asistencia/personal");
        Assert.Equal(HttpStatusCode.OK, allStaffAssistanceResponse.StatusCode);
        using var allStaffAssistance = JsonDocument.Parse(
            await allStaffAssistanceResponse.Content.ReadAsStringAsync());
        Assert.Contains(allStaffAssistance.RootElement.EnumerateArray(), request =>
            request.GetProperty("id").GetInt32() == assistanceId
            && request.GetProperty("estado").GetString() == "Resuelta");

        var scheduleResponse = await client.PostAsJsonAsync("/api/horarios", new
        {
            fecha = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)),
            horaInicio = "09:00:00",
            horaFin = "10:00:00"
        });
        Assert.Equal(HttpStatusCode.Created, scheduleResponse.StatusCode);
        using var scheduleJson = JsonDocument.Parse(await scheduleResponse.Content.ReadAsStringAsync());
        var scheduleId = scheduleJson.RootElement.GetProperty("id").GetInt32();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", registered.AccessToken);
        var enquiryResponse = await client.PostAsJsonAsync("/api/solicitudes-asesoria", new
        {
            tipoId = 1,
            horarioId = scheduleId,
            presupuesto = 200,
            descripcionNecesidad = "Protección familiar"
        });
        Assert.Equal(HttpStatusCode.Created, enquiryResponse.StatusCode);
        using var enquiryJson = JsonDocument.Parse(await enquiryResponse.Content.ReadAsStringAsync());
        var enquiryId = enquiryJson.RootElement.GetProperty("id").GetInt32();
        var forbiddenComparison = await client.GetAsync(
            $"/api/solicitudes-asesoria/{enquiryId}/comparador");
        Assert.Equal(HttpStatusCode.Forbidden, forbiddenComparison.StatusCode);
        var forbiddenOperationalList = await client.GetAsync("/api/solicitudes-asesoria/operativas");
        Assert.Equal(HttpStatusCode.Forbidden, forbiddenOperationalList.StatusCode);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", advisorToken);
        var operationalResponse = await client.GetAsync("/api/solicitudes-asesoria/operativas");
        Assert.Equal(HttpStatusCode.OK, operationalResponse.StatusCode);
        using var operationalJson = JsonDocument.Parse(await operationalResponse.Content.ReadAsStringAsync());
        Assert.Contains(
            operationalJson.RootElement.EnumerateArray(),
            request => request.GetProperty("id").GetInt32() == enquiryId);

        var comparisonResponse = await client.GetAsync(
            $"/api/solicitudes-asesoria/{enquiryId}/comparador");
        Assert.Equal(HttpStatusCode.OK, comparisonResponse.StatusCode);
        using var comparisonJson = JsonDocument.Parse(await comparisonResponse.Content.ReadAsStringAsync());
        var comparison = Assert.Single(comparisonJson.RootElement.EnumerateArray().ToArray());
        Assert.True(comparison.GetProperty("recomendada").GetBoolean());
        var productId = comparison.GetProperty("productoId").GetInt32();

        var confirmResponse = await client.PatchAsJsonAsync(
            $"/api/solicitudes-asesoria/{enquiryId}/estado",
            new { estado = "Confirmada" });
        Assert.Equal(HttpStatusCode.OK, confirmResponse.StatusCode);

        var futureAdvisoryResponse = await client.PostAsJsonAsync("/api/asesorias", new
        {
            solicitudId = enquiryId,
            fechaAsesoria = DateTimeOffset.UtcNow.AddDays(1),
            necesidadesIdentificadas = "Protección familiar"
        });
        Assert.Equal(HttpStatusCode.BadRequest, futureAdvisoryResponse.StatusCode);

        var prematureAdvisoryResponse = await client.PostAsJsonAsync("/api/asesorias", new
        {
            solicitudId = enquiryId,
            fechaAsesoria = DateTimeOffset.UtcNow,
            necesidadesIdentificadas = "Protección familiar"
        });
        Assert.Equal(HttpStatusCode.Conflict, prematureAdvisoryResponse.StatusCode);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SanaliaDbContext>();
            var schedule = await db.Horarios.FindAsync(scheduleId);
            Assert.NotNull(schedule);
            schedule.Fecha = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
            await db.SaveChangesAsync();
        }

        var advisoryResponse = await client.PostAsJsonAsync("/api/asesorias", new
        {
            solicitudId = enquiryId,
            fechaAsesoria = DateTimeOffset.UtcNow,
            necesidadesIdentificadas = "Protección familiar"
        });
        Assert.Equal(HttpStatusCode.Created, advisoryResponse.StatusCode);
        using var advisoryJson = JsonDocument.Parse(await advisoryResponse.Content.ReadAsStringAsync());
        var advisoryId = advisoryJson.RootElement.GetProperty("id").GetInt32();

        var staffAdvisories = await client.GetAsync("/api/asesorias/personal");
        Assert.Equal(HttpStatusCode.OK, staffAdvisories.StatusCode);
        using var staffAdvisoriesJson = JsonDocument.Parse(await staffAdvisories.Content.ReadAsStringAsync());
        Assert.Single(staffAdvisoriesJson.RootElement.EnumerateArray().ToArray());

        var recoveredAdvisory = await client.GetFromJsonAsync<AsesoriaResponse>(
            $"/api/asesorias/solicitud/{enquiryId}");
        Assert.NotNull(recoveredAdvisory);
        Assert.Equal(advisoryId, recoveredAdvisory.Id);

        var quoteResponse = await client.PostAsJsonAsync(
            $"/api/asesorias/{advisoryId}/cotizaciones",
            new
            {
                detalles = new[] { new { productoId = productId, primaCotizada = 125 } },
                observaciones = "Propuesta inicial"
            });
        Assert.Equal(HttpStatusCode.Created, quoteResponse.StatusCode);
        using var quoteJson = JsonDocument.Parse(await quoteResponse.Content.ReadAsStringAsync());
        var quoteId = quoteJson.RootElement.GetProperty("id").GetInt32();

        var quoteSummariesResponse = await client.GetAsync("/api/cotizaciones/mias");
        Assert.Equal(HttpStatusCode.OK, quoteSummariesResponse.StatusCode);
        using var quoteSummariesJson = JsonDocument.Parse(
            await quoteSummariesResponse.Content.ReadAsStringAsync());
        var quoteSummary = Assert.Single(quoteSummariesJson.RootElement.EnumerateArray().ToArray());
        Assert.Equal("Cliente de prueba", quoteSummary.GetProperty("clienteNombre").GetString());
        Assert.Equal("Vida", quoteSummary.GetProperty("tipoSeguro").GetString());
        Assert.Equal(quoteId, quoteSummary.GetProperty("cotizacion").GetProperty("id").GetInt32());

        var presentQuote = await client.PatchAsJsonAsync(
            $"/api/cotizaciones/{quoteId}/estado",
            new { estado = "Presentada" });
        Assert.Equal(HttpStatusCode.OK, presentQuote.StatusCode);

        var adminLogin = await client.PostAsJsonAsync("/api/auth/login/asesor", new
        {
            correo = "admin@example.com",
            contraseña = "AdminPass123!"
        });
        Assert.Equal(HttpStatusCode.OK, adminLogin.StatusCode);
        using var adminJson = JsonDocument.Parse(await adminLogin.Content.ReadAsStringAsync());
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            adminJson.RootElement.GetProperty("accessToken").GetString());

        var adminQuoteResults = await client.GetAsync("/api/cotizaciones/mias");
        Assert.Equal(HttpStatusCode.OK, adminQuoteResults.StatusCode);
        using var adminQuoteJson = JsonDocument.Parse(await adminQuoteResults.Content.ReadAsStringAsync());
        Assert.Single(adminQuoteJson.RootElement.EnumerateArray().ToArray());

        var adminInsurers = await client.GetFromJsonAsync<List<AseguradoraResponse>>("/api/aseguradoras");
        Assert.NotNull(adminInsurers);
        var insurer = await client.PostAsJsonAsync("/api/aseguradoras", new
        {
            nombre = "Aseguradora de administración",
            telefono = "809-555-0110",
            correoContacto = "contacto@admin-seguros.example"
        });
        Assert.Equal(HttpStatusCode.Created, insurer.StatusCode);
        var createdInsurer = await insurer.Content.ReadFromJsonAsync<AseguradoraResponse>();
        Assert.NotNull(createdInsurer);
        Assert.Equal($"/api/aseguradoras/{createdInsurer.Id}", insurer.Headers.Location?.AbsolutePath);
        var loadedInsurer = await client.GetFromJsonAsync<AseguradoraResponse>(
            $"/api/aseguradoras/{createdInsurer.Id}");
        Assert.Equal(createdInsurer.Nombre, loadedInsurer?.Nombre);
        var updatedInsurer = await client.PutAsJsonAsync(
            $"/api/aseguradoras/{createdInsurer.Id}",
            new
            {
                nombre = "Aseguradora editada",
                telefono = (string?)null,
                correoContacto = "contacto@editada.example"
            });
        Assert.Equal(HttpStatusCode.OK, updatedInsurer.StatusCode);
        var deleteLinkedInsurer = await client.DeleteAsync("/api/aseguradoras/1");
        Assert.Equal(HttpStatusCode.Conflict, deleteLinkedInsurer.StatusCode);
        var deleteInsurer = await client.DeleteAsync($"/api/aseguradoras/{createdInsurer.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteInsurer.StatusCode);

        var adminCreatedQuote = await client.PostAsJsonAsync(
            $"/api/asesorias/{advisoryId}/cotizaciones",
            new
            {
                detalles = new[] { new { productoId = productId, primaCotizada = 130 } },
                observaciones = "Propuesta creada por administración"
            });
        Assert.Equal(HttpStatusCode.Created, adminCreatedQuote.StatusCode);

        var duplicateRegistration = await client.PostAsJsonAsync("/api/auth/register", new
        {
            nombre = "Cliente duplicado",
            documentoIdentidad = "TEST-002",
            telefono = "555-0101",
            correo = "cliente@example.com",
            contraseña = "Secret123!"
        });
        Assert.Equal(HttpStatusCode.Conflict, duplicateRegistration.StatusCode);
        Assert.Equal("application/problem+json", duplicateRegistration.Content.Headers.ContentType?.MediaType);

        var numericEnum = await client.PatchAsJsonAsync("/api/solicitudes-asesoria/1/estado", new
        {
            estado = 999
        });
        Assert.Equal(HttpStatusCode.BadRequest, numericEnum.StatusCode);

        var invalidRegistration = await client.PostAsJsonAsync("/api/auth/register", new
        {
            nombre = "",
            documentoIdentidad = "TEST-002",
            telefono = "555-0101",
            correo = "no-es-correo",
            contraseña = "short"
        });
        Assert.Equal(HttpStatusCode.BadRequest, invalidRegistration.StatusCode);
    }

    private async Task SeedCatalogAsync()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SanaliaDbContext>();
        if (await db.TiposSeguro.AnyAsync())
            return;

        var tipo = new TipoSeguro { Nombre = "Vida", Descripcion = "Seguro de vida" };
        var aseguradora = new Aseguradora { Nombre = "Aseguradora de prueba" };
        db.ProductosSeguro.Add(new ProductoSeguro
        {
            Nombre = "Vida Integral",
            TipoSeguro = tipo,
            Aseguradora = aseguradora,
            Prima = 100,
            Deducible = 10,
            NivelCobertura = NivelCobertura.Completo,
            Vigente = true
        });
        await db.SaveChangesAsync();
    }
}
