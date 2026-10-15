namespace Sanalia.Core.Entities;

/// <summary>Categoría de seguro: vehículos, vida, salud, propiedad, etc.</summary>
public class TipoSeguro
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }

    public ICollection<ProductoSeguro> Productos { get; set; } = new List<ProductoSeguro>();
    public ICollection<SolicitudAsesoria> Solicitudes { get; set; } = new List<SolicitudAsesoria>();
}