using System;

namespace Sanalia.Core.Interfaces;

public interface IDataSeeder
{
    /// <summary>Carga datos iniciales. Es seguro llamarlo varias veces: no duplica lo que ya existe.</summary>
    Task SeedAsync(CancellationToken ct = default);
}
