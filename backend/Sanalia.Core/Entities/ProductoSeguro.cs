using Sanalia.Core.Enums;

namespace Sanalia.Core.Entities;

/// <summary>Plan de seguro concreto de una aseguradora; es lo que se compara y se cotiza.</summary>
public class ProductoSeguro
{
    public int Id { get; set; }
    public int AseguradoraId { get; set; }
    public int TipoId { get; set; }
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Precio mensual.</summary>
    public decimal Prima { get; set; }

    /// <summary>Monto que paga el cliente en cada reclamación.</summary>
    public decimal Deducible { get; set; }

    public NivelCobertura NivelCobertura { get; set; }
    public string? BeneficiosAdicionales { get; set; }

    /// <summary>Si es false, el producto no aparece en comparaciones ni cotizaciones nuevas.</summary>
    public bool Vigente { get; set; } = true;

    public Aseguradora Aseguradora { get; set; } = null!;
    public TipoSeguro TipoSeguro { get; set; } = null!;
    public ICollection<Cobertura> Coberturas { get; set; } = new List<Cobertura>();
    public ICollection<DetalleCotizacion> Detalles { get; set; } = new List<DetalleCotizacion>();
}