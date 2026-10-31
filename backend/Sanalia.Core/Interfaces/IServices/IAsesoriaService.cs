using Sanalia.Core.Entities;

namespace Sanalia.Core.Interfaces.IServices;

public interface IAsesoriaService
{
    Task<Asesoria> CrearAsync(
        int idSolicitud,
        int idAsesor,
        DateTime fechaAsesoria,
        string? necesidadesIdentificadas = null,
        string? notas = null,
        CancellationToken ct = default);

    Task<IReadOnlyList<Asesoria>> GetHistorialPorClienteAsync(int idCliente, CancellationToken ct = default);
}
