using System.Linq.Expressions;
using Sanalia.Core.Entities;

namespace Sanalia.Core.Interfaces;

/// <summary>
/// Operaciones comunes para cualquier entidad con llave entera. Los métodos de lectura devuelven
/// entidades sin seguimiento (AsNoTracking) salvo GetByIdAsync, que sí las rastrea para poder
/// modificarlas y llamar a SaveChangesAsync.
/// </summary>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

public interface IClienteRepository : IRepository<Cliente>
{
    Task<Cliente?> GetByCorreoAsync(string correo, CancellationToken ct = default);
    Task<Cliente?> GetByDocumentoAsync(string documentoIdentidad, CancellationToken ct = default);

    /// <summary>Busca por nombre, documento, correo o teléfono (coincidencia parcial).</summary>
    Task<IReadOnlyList<Cliente>> BuscarAsync(string texto, CancellationToken ct = default);
}

public interface IAsesorRepository : IRepository<Asesor>
{
    Task<Asesor?> GetByCorreoAsync(string correo, CancellationToken ct = default);
}

public interface IProductoSeguroRepository : IRepository<ProductoSeguro>
{
    /// <summary>Productos vigentes de un tipo, con aseguradora y coberturas. Base del comparador (P6).</summary>
    Task<IReadOnlyList<ProductoSeguro>> GetVigentesPorTipoAsync(int idTipo, CancellationToken ct = default);

    /// <summary>Un producto con aseguradora, tipo y coberturas.</summary>
    Task<ProductoSeguro?> GetConDetalleAsync(int idProducto, CancellationToken ct = default);
}

public interface IHorarioDisponibleRepository : IRepository<HorarioDisponible>
{
    /// <summary>Horarios libres desde una fecha, opcionalmente de un asesor, ordenados por fecha y hora.</summary>
    Task<IReadOnlyList<HorarioDisponible>> GetDisponiblesAsync(DateOnly desde, int? idAsesor = null, CancellationToken ct = default);

    /// <summary>
    /// Marca el horario como no disponible solo si todavía estaba libre, en una única operación
    /// atómica en la base de datos. Devuelve false si otro cliente lo tomó antes.
    /// No usa SaveChangesAsync: el cambio se aplica de inmediato.
    /// </summary>
    Task<bool> TryReservarAsync(int idHorario, CancellationToken ct = default);

    /// <summary>Vuelve a dejar libre un horario (por ejemplo, al cancelar una solicitud).</summary>
    Task LiberarAsync(int idHorario, CancellationToken ct = default);
}

public interface ISolicitudAsesoriaRepository : IRepository<SolicitudAsesoria>
{
    /// <summary>Solicitudes pendientes con cliente, tipo y horario, de la más antigua a la más nueva.</summary>
    Task<IReadOnlyList<SolicitudAsesoria>> GetPendientesAsync(CancellationToken ct = default);

    Task<SolicitudAsesoria?> GetConDetalleAsync(int idSolicitud, CancellationToken ct = default);

    Task<IReadOnlyList<SolicitudAsesoria>> GetByClienteAsync(int idCliente, CancellationToken ct = default);
}

public interface ISolicitudAsistenciaRepository : IRepository<SolicitudAsistencia>
{
    /// <summary>Asistencias que no están resueltas, con el cliente y el asesor asignado.</summary>
    Task<IReadOnlyList<SolicitudAsistencia>> GetAbiertasAsync(CancellationToken ct = default);
}

public interface IAsesoriaRepository : IRepository<Asesoria>
{
    /// <summary>Historial de un cliente (P8): asesorías con sus cotizaciones y alternativas, la más reciente primero.</summary>
    Task<IReadOnlyList<Asesoria>> GetHistorialPorClienteAsync(int idCliente, CancellationToken ct = default);
}

public interface ICotizacionRepository : IRepository<Cotizacion>
{
    /// <summary>Una cotización con sus detalles, productos y aseguradoras.</summary>
    Task<Cotizacion?> GetConDetallesAsync(int idCotizacion, CancellationToken ct = default);
}