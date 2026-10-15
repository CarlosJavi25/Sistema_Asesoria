namespace Sanalia.Core.Entities;

/// <summary>Una alternativa (producto) incluida en una cotización, con su puntaje de coincidencia.</summary>
public class DetalleCotizacion
{
    public int Id { get; set; }
    public int CotizacionId { get; set; }
    public int ProductoId { get; set; }

    /// <summary>Precio cotizado al cliente; puede diferir de la prima de lista del producto.</summary>
    public decimal PrimaCotizada { get; set; }

    /// <summary>Qué tan bien se ajusta al cliente, de 0 a 100. Lo calcula el comparador (proceso P6).</summary>
    public decimal? PuntajeCoincidencia { get; set; }

    public bool Recomendada { get; set; }

    public Cotizacion Cotizacion { get; set; } = null!;
    public ProductoSeguro Producto { get; set; } = null!;
}