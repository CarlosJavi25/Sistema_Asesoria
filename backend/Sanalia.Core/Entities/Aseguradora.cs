namespace Sanalia.Core.Entities;

/// <summary>Compañía de seguros cuyos productos ofrece la corredora.</summary>
public class Aseguradora
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? CorreoContacto { get; set; }

    public ICollection<ProductoSeguro> Productos { get; set; } = new List<ProductoSeguro>();
}