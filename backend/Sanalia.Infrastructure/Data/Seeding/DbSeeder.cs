using Sanalia.Core.Entities;
using Sanalia.Core.Enums;
using Sanalia.Core.Interfaces;
using Sanalia.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Sanalia.Infrastructure.Data.Seeding;

public class DataSeeder(
    SanaliaDbContext db,
    IPasswordHasher hasher,
    IOptions<DemoSeedCredentials> credentials) : IDataSeeder
{
    // SOLO DESARROLLO. El seeder se ejecuta únicamente en el entorno Development (ver Program.cs).
    private readonly SanaliaDbContext _db = db;
    private readonly IPasswordHasher _hasher = hasher;
    private readonly DemoSeedCredentials _credentials = credentials.Value;

    public async Task SeedAsync(CancellationToken ct = default)
    {
        // Cada bloque se salta si su tabla ya tiene datos, así que se puede repetir sin duplicar.
        if (!await _db.Asesores.AnyAsync(ct))
            await SeedAsesoresAsync(ct);

        if (!await _db.TiposSeguro.AnyAsync(ct))
            await SeedCatalogoAsync(ct);

        if (!await _db.Clientes.AnyAsync(ct))
            await SeedDemoAsync(ct);
    }

    // ───────────────────────── Usuarios del sistema web ─────────────────────────

    private async Task SeedAsesoresAsync(CancellationToken ct)
    {
        _credentials.ValidateAdminAndAdvisor();
        _db.Asesores.AddRange(
            new Asesor
            {
                Nombre = "Administrador Sanalia",
                Correo = "admin@sanalia.local",
                Contraseña = _hasher.Hash(_credentials.AdminPassword),
                Rol = RolUsuario.Administrador,
                Activo = true
            },
            new Asesor
            {
                Nombre = "Asesor Demo",
                Correo = "asesor@sanalia.local",
                Contraseña = _hasher.Hash(_credentials.AdvisorPassword),
                Rol = RolUsuario.Asesor,
                Activo = true
            });

        await _db.SaveChangesAsync(ct);
    }

    // ───────────────────────── Catálogo de seguros ─────────────────────────

    private async Task SeedCatalogoAsync(CancellationToken ct)
    {
        var quisqueya = new Aseguradora { Nombre = "Seguros Quisqueya (demo)", Telefono = "809-555-0101", CorreoContacto = "contacto@quisqueya.demo" };
        var caribe = new Aseguradora { Nombre = "Aseguradora del Caribe (demo)", Telefono = "809-555-0102", CorreoContacto = "contacto@caribe.demo" };
        var proteccion = new Aseguradora { Nombre = "Protección Dominicana (demo)", Telefono = "809-555-0103", CorreoContacto = "contacto@proteccion.demo" };

        var vehiculos = new TipoSeguro { Nombre = "Vehículos", Descripcion = "Autos, jeepetas y motocicletas" };
        var vida = new TipoSeguro { Nombre = "Vida", Descripcion = "Protección para el asegurado y su familia" };
        var salud = new TipoSeguro { Nombre = "Salud", Descripcion = "Gastos médicos y hospitalización" };
        var propiedad = new TipoSeguro { Nombre = "Propiedad", Descripcion = "Viviendas y negocios" };

        var productos = new List<ProductoSeguro>
        {
            // Vehículos
            Producto(quisqueya, vehiculos, "Auto Básico (Ley)", 1_200, 0, NivelCobertura.Basico, null,
                Cob("Daños a la propiedad de terceros", 500_000),
                Cob("Lesiones a terceros", 1_000_000)),
            Producto(caribe, vehiculos, "Auto Plus", 2_600, 15_000, NivelCobertura.Intermedio, "Asistencia vial 24/7",
                Cob("Daños a la propiedad de terceros", 1_500_000),
                Cob("Lesiones a terceros", 2_000_000),
                Cob("Robo total del vehículo", null, "Hasta el valor asegurado")),
            Producto(proteccion, vehiculos, "Auto Total", 4_300, 10_000, NivelCobertura.Completo, "Vehículo de reemplazo; asistencia vial; cristales",
                Cob("Todo riesgo (colisión, vuelco, incendio)", null, "Hasta el valor asegurado"),
                Cob("Daños a la propiedad de terceros", 3_000_000),
                Cob("Lesiones a terceros", 3_000_000)),

            // Vida
            Producto(quisqueya, vida, "Vida Temporal", 900, 0, NivelCobertura.Basico, null,
                Cob("Fallecimiento por cualquier causa", 1_000_000)),
            Producto(caribe, vida, "Vida Protegida", 1_700, 0, NivelCobertura.Intermedio, "Invalidez total y permanente",
                Cob("Fallecimiento por cualquier causa", 2_500_000),
                Cob("Invalidez total y permanente", 2_500_000)),
            Producto(proteccion, vida, "Vida Integral", 3_200, 0, NivelCobertura.Completo, "Enfermedades graves; gastos funerarios",
                Cob("Fallecimiento por cualquier causa", 5_000_000),
                Cob("Enfermedades graves", 1_500_000),
                Cob("Gastos funerarios", 150_000)),

            // Salud
            Producto(quisqueya, salud, "Salud Esencial", 1_800, 2_000, NivelCobertura.Basico, "Telemedicina",
                Cob("Consultas ambulatorias (por año)", 50_000),
                Cob("Emergencias", 200_000)),
            Producto(caribe, salud, "Salud Familiar", 3_900, 1_000, NivelCobertura.Intermedio, "Chequeo anual; telemedicina",
                Cob("Hospitalización", 1_000_000),
                Cob("Maternidad", 150_000, "Período de espera de 10 meses"),
                Cob("Consultas ambulatorias (por año)", 100_000)),
            Producto(proteccion, salud, "Salud Premium", 7_500, 0, NivelCobertura.Completo, "Red internacional; chequeo anual; odontología básica",
                Cob("Hospitalización", 5_000_000),
                Cob("Medicamentos ambulatorios", 100_000),
                Cob("Cobertura en el exterior", 2_000_000)),

            // Propiedad
            Producto(quisqueya, propiedad, "Hogar Básico", 1_100, 5_000, NivelCobertura.Basico, null,
                Cob("Incendio y rayo", 3_000_000)),
            Producto(caribe, propiedad, "Hogar Seguro", 2_300, 8_000, NivelCobertura.Intermedio, "Asistencia de plomería y cerrajería",
                Cob("Incendio y rayo", 5_000_000),
                Cob("Robo de contenido", 500_000)),
            Producto(proteccion, propiedad, "Hogar Total", 3_800, 10_000, NivelCobertura.Completo, "Cobertura de huracán e inundación",
                Cob("Incendio y rayo", 8_000_000),
                Cob("Fenómenos naturales (huracán, inundación)", 8_000_000),
                Cob("Robo de contenido", 1_000_000)),
        };

        // Basta con agregar los productos: EF descubre aseguradoras, tipos y coberturas por las navegaciones.
        _db.ProductosSeguro.AddRange(productos);
        await _db.SaveChangesAsync(ct);
    }

    // ───────────────────────── Clientes, horarios y solicitudes de ejemplo ─────────────────────────

    private async Task SeedDemoAsync(CancellationToken ct)
    {
        _credentials.ValidateClient();
        var asesor = await _db.Asesores.FirstAsync(a => a.Rol == RolUsuario.Asesor, ct);
        var tipoVehiculos = await _db.TiposSeguro.FirstAsync(t => t.Nombre == "Vehículos", ct);

        var cliente1 = new Cliente
        {
            Nombre = "María Pérez (demo)",
            DocumentoIdentidad = "00100000001",
            Telefono = "809-555-0201",
            Correo = "cliente1@demo.local",
            Contraseña = _hasher.Hash(_credentials.ClientPassword)
        };
        var cliente2 = new Cliente
        {
            Nombre = "Luis Gómez (demo)",
            DocumentoIdentidad = "00100000002",
            Telefono = "809-555-0202",
            Correo = "cliente2@demo.local",
            Contraseña = _hasher.Hash(_credentials.ClientPassword)
        };

        // Horarios: los próximos 5 días laborables, 4 franjas por día.
        var franjas = new[] { (9, 10), (10, 11), (14, 15), (15, 16) };
        var horarios = new List<HorarioDisponible>();
        var dia = DateOnly.FromDateTime(DateTime.UtcNow);
        while (horarios.Count < 20)
        {
            dia = dia.AddDays(1);
            if (dia.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                continue;

            foreach (var (inicio, fin) in franjas)
            {
                horarios.Add(new HorarioDisponible
                {
                    Asesor = asesor,
                    Fecha = dia,
                    HoraInicio = new TimeOnly(inicio, 0),
                    HoraFin = new TimeOnly(fin, 0),
                    Disponible = true
                });
            }
        }

        // Un cliente ya pidió asesoría y reservó la primera franja.
        horarios[0].Disponible = false;
        var solicitud = new SolicitudAsesoria
        {
            Cliente = cliente1,
            TipoSeguro = tipoVehiculos,
            DescripcionNecesidad = "Necesito asegurar mi vehículo.",
            Presupuesto = 3_000,
            Horario = horarios[0],
            Estado = EstadoSolicitudAsesoria.Pendiente
        };

        // Otro cliente pidió asistencia general, sin asesor asignado todavía.
        var asistencia = new SolicitudAsistencia
        {
            Cliente = cliente2,
            Motivo = "No sé qué tipo de seguro me conviene, ¿pueden llamarme?",
            Estado = EstadoAsistencia.Pendiente
        };

        _db.Horarios.AddRange(horarios);
        _db.SolicitudesAsesoria.Add(solicitud);
        _db.SolicitudesAsistencia.Add(asistencia);
        await _db.SaveChangesAsync(ct);
    }


    private static ProductoSeguro Producto(
           Aseguradora aseguradora, TipoSeguro tipo, string nombre,
           decimal prima, decimal deducible, NivelCobertura nivel, string? beneficios,
           params Cobertura[] coberturas) => new()
           {
               Aseguradora = aseguradora,
               TipoSeguro = tipo,
               Nombre = nombre,
               Prima = prima,
               Deducible = deducible,
               NivelCobertura = nivel,
               BeneficiosAdicionales = beneficios,
               Vigente = true,
               Coberturas = coberturas.ToList()
           };

    private static Cobertura Cob(string descripcion, decimal? limite, string? condiciones = null) => new()
    {
        Descripcion = descripcion,
        Limite = limite,
        Condiciones = condiciones
    };
}
