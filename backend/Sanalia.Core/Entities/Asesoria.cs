namespace Sanalia.Core.Entities;

/// <summary>Sesión de asesoría realizada por un asesor a un cliente.</summary>
public class Asesoria
{
    public int Id { get; set; }
    public int SolicitudId { get; set; }
    public int ClienteId { get; set; }
    public int AsesorId { get; set; }
    public DateTime FechaAsesoria { get; set; }
    public string? NecesidadesIdentificadas { get; set; }
    public string? Notas { get; set; }

    public SolicitudAsesoria Solicitud { get; set; } = null!;
    public Cliente Cliente { get; set; } = null!;
    public Asesor Asesor { get; set; } = null!;
    public ICollection<Cotizacion> Cotizaciones { get; set; } = new List<Cotizacion>();
}