using Sanalia.Core.Enums;

namespace Sanalia.Core.Entities;

/// <summary>Propuesta que el asesor genera dentro de una asesoría con las alternativas evaluadas.</summary>
public class Cotizacion
{
    public int Id { get; set; }
    public int AsesoriaId { get; set; }
    public DateOnly FechaEmision { get; set; }
    public EstadoCotizacion Estado { get; set; } = EstadoCotizacion.Borrador;
    public string? Observaciones { get; set; }

    public Asesoria Asesoria { get; set; } = null!;
    public ICollection<DetalleCotizacion> Detalles { get; set; } = new List<DetalleCotizacion>();
}