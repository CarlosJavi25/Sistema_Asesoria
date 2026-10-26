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
            db.Asesores.Add(new Asesor
            {
                Nombre = "Asesora de prueba",
                Correo = "asesora@example.com",
                Contraseña = hasher.Hash("AdvisorPass123!"),
                Rol = RolUsuario.Asesor
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
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                advisorJson.RootElement.GetProperty("accessToken").GetString());

        var customerDirectory = await client.GetFromJsonAsync<List<ClienteResponse>>(
            "/api/clientes?busqueda=Cliente%20de%20prueba");
        Assert.NotNull(customerDirectory);
        var listedCustomer = Assert.Single(customerDirectory);
        Assert.Equal("Cliente de prueba", listedCustomer.Nombre);
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
        Assert.Equal("Pendiente", assistanceJson.RootElement.GetProperty("estado").GetString());

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
