using Sanalia.Core.Enums;

namespace Sanalia.Api.Contracts;

/// <summary>Tipo de seguro disponible en el catálogo.</summary>
public sealed record TipoSeguroResponse(int Id, string Nombre, string? Descripcion);

/// <summary>Aseguradora que ofrece productos en el catálogo.</summary>
public sealed record AseguradoraResponse(int Id, string Nombre, string? Telefono, string? CorreoContacto);

/// <summary>Detalle de una cobertura incluida en un producto.</summary>
public sealed record CoberturaResponse(int Id, string Descripcion, decimal? Limite, string? Condiciones);

/// <summary>Producto de seguro con información de la aseguradora, tipo y coberturas.</summary>
public sealed record ProductoSeguroResponse(
    int Id,
    string Nombre,
    decimal Prima,
    decimal Deducible,
    NivelCobertura NivelCobertura,
    string? BeneficiosAdicionales,
    bool Vigente,
    AseguradoraResponse Aseguradora,
    TipoSeguroResponse TipoSeguro,
    IReadOnlyList<CoberturaResponse> Coberturas);

/// <summary>Alternativa recomendada del comparador para una solicitud.</summary>
public sealed record AlternativaSeguroResponse(
    int ProductoId,
    decimal PrimaCotizada,
    decimal? PuntajeCoincidencia,
    bool Recomendada,
    ProductoSeguroResponse Producto);
