using Sanalia.Core.Enums;

namespace Sanalia.Core.Entities;

/// <summary>Pedido de ayuda u orientación del cliente, distinto de la solicitud de asesoría.</summary>
public class SolicitudAsistencia
{
    public int Id { get; set; }
    public int ClienteId { get; set; }

    /// <summary>Asesor que le da seguimiento; null mientras nadie la haya tomado.</summary>
    public int? AsesorId { get; set; }

    public string Motivo { get; set; } = string.Empty;
    public EstadoAsistencia Estado { get; set; } = EstadoAsistencia.Pendiente;
    public DateTime FechaSolicitud { get; set; } = DateTime.UtcNow;

    public Cliente Cliente { get; set; } = null!;
    public Asesor? Asesor { get; set; }
}