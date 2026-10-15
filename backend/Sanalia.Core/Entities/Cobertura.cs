namespace Sanalia.Core.Entities;

/// <summary>Qué cubre un producto, hasta cuánto y bajo qué condiciones.</summary>
public class Cobertura
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public decimal? Limite { get; set; }
    public string? Condiciones { get; set; }

    public ProductoSeguro Producto { get; set; } = null!;
}