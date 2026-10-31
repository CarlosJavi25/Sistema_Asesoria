namespace Sanalia.Core.Entities;

public class Cliente
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string DocumentoIdentidad { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Contraseña { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }

    public ICollection<SolicitudAsesoria> SolicitudesAsesoria { get; set; } = new List<SolicitudAsesoria>();
    public ICollection<SolicitudAsistencia> SolicitudesAsistencia { get; set; } = new List<SolicitudAsistencia>();
    public ICollection<Asesoria> Asesorias { get; set; } = new List<Asesoria>();
}
