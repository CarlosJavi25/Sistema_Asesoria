using Sanalia.Core.Enums;

namespace Sanalia.Core.Entities;

/// <summary>Solicitud que el cliente hace desde la app: tipo de seguro, necesidad y horario elegido.</summary>
public class SolicitudAsesoria
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public int TipoId { get; set; }
    public string? DescripcionNecesidad { get; set; }

    /// <summary>Horario reservado. Es único: un horario no puede ser tomado por dos solicitudes.</summary>
    public int? HorarioId { get; set; }

    public EstadoSolicitudAsesoria Estado { get; set; } = EstadoSolicitudAsesoria.Pendiente;
    public DateTime FechaSolicitud { get; set; } = DateTime.UtcNow;

    public Cliente Cliente { get; set; } = null!;
    public TipoSeguro TipoSeguro { get; set; } = null!;
    public HorarioDisponible? Horario { get; set; }

    /// <summary>Asesoría que se originó a partir de esta solicitud (1 a 1 opcional).</summary>
    public Asesoria? Asesoria { get; set; }
}