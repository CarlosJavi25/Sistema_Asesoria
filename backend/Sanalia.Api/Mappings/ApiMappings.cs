using Sanalia.Api.Contracts;
using Sanalia.Core.Entities;
using Sanalia.Core.Models;

namespace Sanalia.Api.Mappings;

internal static class ApiMappings
{
    public static CurrentUserResponse ToResponse(this UserIdentity identity) =>
        new(identity.Id, identity.Nombre, identity.Correo, identity.EsAsesor, identity.Rol);

    public static TipoSeguroResponse ToResponse(this TipoSeguro tipo) =>
        new(tipo.Id, tipo.Nombre, tipo.Descripcion);

    public static AseguradoraResponse ToResponse(this Aseguradora aseguradora) =>
        new(aseguradora.Id, aseguradora.Nombre, aseguradora.Telefono, aseguradora.CorreoContacto);

    public static ProductoSeguroResponse ToResponse(this ProductoSeguro producto) =>
        new(
            producto.Id,
            producto.Nombre,
            producto.Prima,
            producto.Deducible,
            producto.NivelCobertura,
            producto.BeneficiosAdicionales,
            producto.Vigente,
            producto.Aseguradora.ToResponse(),
            new TipoSeguroResponse(producto.TipoSeguro.Id, producto.TipoSeguro.Nombre, producto.TipoSeguro.Descripcion),
            producto.Coberturas
                .Select(c => new CoberturaResponse(c.Id, c.Descripcion, c.Limite, c.Condiciones))
                .ToArray());

    public static HorarioDisponibleResponse ToResponse(this HorarioDisponible horario) =>
        new(
            horario.Id,
            horario.AsesorId,
            horario.Asesor?.Nombre ?? string.Empty,
            horario.Fecha,
            horario.HoraInicio,
            horario.HoraFin);

    public static SolicitudAsesoriaResponse ToResponse(this SolicitudAsesoria solicitud) =>
        new(
            solicitud.Id,
            solicitud.ClienteId,
            solicitud.Cliente is null ? null : solicitud.Cliente.ToSummary(),
            solicitud.TipoId,
            solicitud.TipoSeguro?.Nombre ?? string.Empty,
            solicitud.DescripcionNecesidad,
            solicitud.Presupuesto,
            solicitud.HorarioId,
            solicitud.Horario is null ? null : solicitud.Horario.ToResponse(),
            solicitud.Estado,
            UtcOffset(solicitud.FechaSolicitud));

    public static SolicitudAsistenciaResponse ToResponse(this SolicitudAsistencia solicitud) =>
        new(
            solicitud.Id,
            solicitud.ClienteId,
            solicitud.Cliente is null ? null : solicitud.Cliente.ToSummary(),
            solicitud.AsesorId,
            solicitud.Asesor?.Nombre,
            solicitud.Motivo,
            solicitud.Estado,
            UtcOffset(solicitud.FechaSolicitud));

    public static AsesoriaResponse ToResponse(this Asesoria asesoria) =>
        new(
            asesoria.Id,
            asesoria.SolicitudId,
            asesoria.ClienteId,
            asesoria.AsesorId,
            UtcOffset(asesoria.FechaAsesoria),
            asesoria.NecesidadesIdentificadas,
            asesoria.Notas,
            asesoria.Cotizaciones.Select(ToResponse).ToArray());

    public static CotizacionResponse ToResponse(this Cotizacion cotizacion) =>
        new(
            cotizacion.Id,
            cotizacion.AsesoriaId,
            cotizacion.FechaEmision,
            cotizacion.Estado,
            cotizacion.Observaciones,
            cotizacion.Detalles.Select(ToResponse).ToArray());

    public static AlternativaSeguroResponse ToResponse(this DetalleCotizacion detalle) =>
        new(
            detalle.ProductoId,
            detalle.PrimaCotizada,
            detalle.PuntajeCoincidencia,
            detalle.Recomendada,
            detalle.Producto.ToResponse());

    private static ClienteSummaryResponse ToSummary(this Cliente cliente) =>
        new(cliente.Id, cliente.Nombre, cliente.Correo, cliente.Telefono);

    private static DateTimeOffset UtcOffset(DateTime value) =>
        new(DateTime.SpecifyKind(value, DateTimeKind.Utc));
}
