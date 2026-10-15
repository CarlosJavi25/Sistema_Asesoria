namespace Sanalia.Core.Entities;

/// <summary>Franja horaria que un asesor ofrece para atender una asesoría.</summary>
public class HorarioDisponible
{
    public int Id { get; set; }
    public int AsesorId { get; set; }
    public DateOnly Fecha { get; set; }
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }

    /// <summary>False cuando ya fue reservado por una solicitud.</summary>
    public bool Disponible { get; set; } = true;

    public Asesor Asesor { get; set; } = null!;

    /// <summary>Solicitud que reservó este horario (relación 1 a 1 opcional).</summary>
    public SolicitudAsesoria? Solicitud { get; set; }
}