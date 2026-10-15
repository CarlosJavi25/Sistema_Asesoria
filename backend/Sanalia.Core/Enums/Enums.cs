namespace Sanalia.Core.Enums;

/// <summary>Rol de un usuario del sistema web (tabla asesor).</summary>
public enum RolUsuario
{
    Asesor,
    Administrador
}

/// <summary>Nivel de cobertura de un producto de seguro.</summary>
public enum NivelCobertura
{
    Basico,
    Intermedio,
    Completo
}

/// <summary>Ciclo de vida de una solicitud de asesoría hecha desde la app móvil.</summary>
public enum EstadoSolicitudAsesoria
{
    Pendiente,
    Confirmada,
    Atendida,
    Cancelada
}

/// <summary>Ciclo de vida de una solicitud de asistencia.</summary>
public enum EstadoAsistencia
{
    Pendiente,
    EnProceso,
    Resuelta
}

/// <summary>Ciclo de vida de una cotización.</summary>
public enum EstadoCotizacion
{
    Borrador,
    Presentada,
    Aceptada,
    Rechazada
}