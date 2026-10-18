using System.ComponentModel.DataAnnotations;
using Sanalia.Core.Entities;
using Sanalia.Core.Interfaces;
using Sanalia.Core.Interfaces.IServices;
using Sanalia.Core.Models;

namespace Sanalia.Infrastructure.Services;

public sealed class AuthService(
    IClienteRepository clientes,
    IAsesorRepository asesores,
    IPasswordHasher passwordHasher) : IAuthService
{
    public async Task<UserIdentity> RegisterClienteAsync(
        string nombre,
        string documentoIdentidad,
        string telefono,
        string correo,
        string contraseña,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        ArgumentException.ThrowIfNullOrWhiteSpace(documentoIdentidad);
        ArgumentException.ThrowIfNullOrWhiteSpace(telefono);
        ArgumentException.ThrowIfNullOrWhiteSpace(contraseña);

        nombre = nombre.Trim();
        documentoIdentidad = documentoIdentidad.Trim();
        telefono = telefono.Trim();
        correo = NormalizarCorreo(correo);

        if (await clientes.GetByCorreoAsync(correo, ct) is not null)
            throw new InvalidOperationException("Ya existe un cliente registrado con ese correo.");

        if (await clientes.GetByDocumentoAsync(documentoIdentidad, ct) is not null)
            throw new InvalidOperationException("Ya existe un cliente registrado con ese documento.");

        var cliente = new Cliente
        {
            Nombre = nombre,
            DocumentoIdentidad = documentoIdentidad,
            Telefono = telefono,
            Correo = correo,
            Contraseña = passwordHasher.Hash(contraseña),
            FechaRegistro = DateTime.UtcNow
        };

        await clientes.AddAsync(cliente, ct);
        await clientes.SaveChangesAsync(ct);

        return new UserIdentity(cliente.Id, cliente.Nombre, cliente.Correo, false, null);
    }

    public async Task<UserIdentity?> AuthenticateClienteAsync(
        string correo,
        string contraseña,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrEmpty(contraseña))
            return null;

        var cliente = await clientes.GetByCorreoAsync(NormalizarCorreo(correo), ct);
        if (cliente is null || !passwordHasher.Verify(contraseña, cliente.Contraseña))
            return null;

        return new UserIdentity(cliente.Id, cliente.Nombre, cliente.Correo, false, null);
    }

    public async Task<UserIdentity?> AuthenticateAsesorAsync(
        string correo,
        string contraseña,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrEmpty(contraseña))
            return null;

        var asesor = await asesores.GetByCorreoAsync(NormalizarCorreo(correo), ct);
        if (asesor is null || !asesor.Activo || !passwordHasher.Verify(contraseña, asesor.Contraseña))
            return null;

        return new UserIdentity(asesor.Id, asesor.Nombre, asesor.Correo, true, asesor.Rol);
    }

    private static string NormalizarCorreo(string correo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(correo);
        var normalizado = correo.Trim().ToLowerInvariant();
        if (!new EmailAddressAttribute().IsValid(normalizado))
            throw new ArgumentException("El correo no tiene un formato válido.", nameof(correo));

        return normalizado;
    }
}
