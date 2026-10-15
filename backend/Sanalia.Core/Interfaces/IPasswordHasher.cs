using System;

namespace Sanalia.Core.Interfaces;

public interface IPasswordHasher
{
    /// <summary>Devuelve el hash con sal para guardar en clave_hash.</summary>
    string Hash(string password);

    /// <summary>Compara una contraseña en texto con un hash guardado.</summary>
    bool Verify(string password, string hash);
}
