using Sanalia.Core.Enums;

namespace Sanalia.Core.Entities;

public class Asesor()
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Contraseña { get; set; } = string.Empty;
    public RolUsuario Rol { get; set; } = RolUsuario.Asesor;

    /// <summary>Equivale al campo "estado" del diccionario de datos (activo/inactivo).</summary>
    public bool Activo { get; set; } = true;

    public ICollection<HorarioDisponible> Horarios { get; set; } = new List<HorarioDisponible>();
    public ICollection<Asesoria> Asesorias { get; set; } = new List<Asesoria>();
    public ICollection<SolicitudAsistencia> Asistencias { get; set; } = new List<SolicitudAsistencia>();
}
