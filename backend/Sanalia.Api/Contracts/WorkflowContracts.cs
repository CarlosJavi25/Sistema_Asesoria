using System.ComponentModel.DataAnnotations;
using Sanalia.Core.Enums;

namespace Sanalia.Api.Contracts;

/// <summary>Disponibilidad ofrecida por un asesor.</summary>
public sealed record HorarioDisponibleResponse(
    int Id,
    int AsesorId,
    string AsesorNombre,
    DateOnly Fecha,
    TimeOnly HoraInicio,
    TimeOnly HoraFin);

/// <summary>Datos para publicar una nueva franja de disponibilidad.</summary>
public sealed record CreateHorarioRequest
{
    [Required]
    public required DateOnly Fecha { get; init; }

    [Required]
    public required TimeOnly HoraInicio { get; init; }

    [Required]
    public required TimeOnly HoraFin { get; init; }
}

/// <summary>Datos para solicitar una asesoría y reservar un horario disponible.</summary>
public sealed record CreateSolicitudAsesoriaRequest
{
    [Range(1, int.MaxValue)]
    public required int TipoId { get; init; }

    [Range(1, int.MaxValue)]
    public required int HorarioId { get; init; }

    [Range(typeof(decimal), "0", "999999999999")]
    public decimal? Presupuesto { get; init; }

    [MaxLength(2000)]
    public string? DescripcionNecesidad { get; init; }
}

/// <summary>Estado de destino para una solicitud de asesoría.</summary>
public sealed record ChangeSolicitudAsesoriaStateRequest
{
    [Required]
    public required EstadoSolicitudAsesoria Estado { get; init; }
}

/// <summary>Información de cliente apta para mostrar en operaciones del asesor.</summary>
public sealed record ClienteSummaryResponse(int Id, string Nombre, string Correo, string Telefono);

/// <summary>Datos de contacto de un cliente para el directorio del personal.</summary>
public sealed record ClienteResponse(int Id, string Nombre, string Correo, string Telefono);

/// <summary>Solicitud de asesoría y datos de agenda y tipo de seguro.</summary>
public sealed record SolicitudAsesoriaResponse(
    int Id,
    int ClienteId,
    ClienteSummaryResponse? Cliente,
    int TipoId,
    string TipoSeguro,
    string? DescripcionNecesidad,
    decimal? Presupuesto,
    int? HorarioId,
    HorarioDisponibleResponse? Horario,
    EstadoSolicitudAsesoria Estado,
    DateTimeOffset FechaSolicitud);

/// <summary>Datos para crear una sesión de asesoría desde una solicitud confirmada.</summary>
public sealed record CreateAsesoriaRequest
{
    [Range(1, int.MaxValue)]
    public required int SolicitudId { get; init; }

    [Required]
    public required DateTimeOffset FechaAsesoria { get; init; }

    [MaxLength(2000)]
    public string? NecesidadesIdentificadas { get; init; }

    [MaxLength(4000)]
    public string? Notas { get; init; }
}

/// <summary>Sesión de asesoría y sus cotizaciones generadas.</summary>
public sealed record AsesoriaResponse(
    int Id,
    int SolicitudId,
    int ClienteId,
    int AsesorId,
    DateTimeOffset FechaAsesoria,
    string? NecesidadesIdentificadas,
    string? Notas,
    IReadOnlyList<CotizacionResponse> Cotizaciones);

/// <summary>Datos de una alternativa seleccionada para una cotización.</summary>
public sealed record CreateDetalleCotizacionRequest
{
    [Range(1, int.MaxValue)]
    public required int ProductoId { get; init; }

    [Range(typeof(decimal), "0", "999999999999")]
    public required decimal PrimaCotizada { get; init; }
}

/// <summary>Datos para emitir una cotización a partir de una asesoría.</summary>
public sealed record CreateCotizacionRequest
{
    [Required, MinLength(1)]
    public required IReadOnlyList<CreateDetalleCotizacionRequest> Detalles { get; init; }

    [MaxLength(4000)]
    public string? Observaciones { get; init; }
}

/// <summary>Estado de destino para una cotización presentada.</summary>
public sealed record ChangeCotizacionStateRequest
{
    [Required]
    public required EstadoCotizacion Estado { get; init; }
}

/// <summary>Cotización con sus alternativas de producto.</summary>
public sealed record CotizacionResponse(
    int Id,
    int AsesoriaId,
    DateOnly FechaEmision,
    EstadoCotizacion Estado,
    string? Observaciones,
    IReadOnlyList<AlternativaSeguroResponse> Detalles);

/// <summary>Solicitud para recibir asistencia de un asesor.</summary>
public sealed record CreateSolicitudAsistenciaRequest
{
    [Required, MinLength(1), MaxLength(2000)]
    public required string Motivo { get; init; }
}

/// <summary>Estado de destino para una solicitud de asistencia asignada.</summary>
public sealed record ChangeAsistenciaStateRequest
{
    [Required]
    public required EstadoAsistencia Estado { get; init; }
}

/// <summary>Solicitud de asistencia con identidad de cliente y asesor asignado.</summary>
public sealed record SolicitudAsistenciaResponse(
    int Id,
    int ClienteId,
    ClienteSummaryResponse? Cliente,
    int? AsesorId,
    string? AsesorNombre,
    string Motivo,
    EstadoAsistencia Estado,
    DateTimeOffset FechaSolicitud);
