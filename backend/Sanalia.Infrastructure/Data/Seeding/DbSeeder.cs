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
        var monumental = new Aseguradora { Nombre = "Seguros Monumental (demo)", Telefono = "809-555-0104", CorreoContacto = "contacto@monumental.demo" };

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
            Producto(monumental, vehiculos, "Auto Ejecutivo", 5_200, 8_000, NivelCobertura.Completo, "Cristales; auto de reemplazo; asistencia jurídica",
                Cob("Todo riesgo (colisión, vuelco, incendio)", null, "Hasta el valor asegurado"),
                Cob("Daños a la propiedad de terceros", 4_000_000),
                Cob("Lesiones a terceros", 4_000_000),
                Cob("Robo total del vehículo", null, "Hasta el valor asegurado")),

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
            Producto(monumental, vida, "Vida Plus", 2_400, 0, NivelCobertura.Completo, "Doble indemnización por accidente; anticipo por enfermedad terminal",
                Cob("Fallecimiento por cualquier causa", 3_500_000),
                Cob("Muerte accidental (beneficio adicional)", 3_500_000),
                Cob("Enfermedades graves", 1_000_000)),

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
            Producto(monumental, salud, "Salud Global", 9_200, 0, NivelCobertura.Completo, "Red internacional; segunda opinión médica; odontología",
                Cob("Hospitalización", 7_000_000),
                Cob("Cobertura en el exterior", 4_000_000),
                Cob("Odontología", 250_000),
                Cob("Medicamentos ambulatorios", 150_000)),

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
            Producto(monumental, propiedad, "Hogar Premium", 4_600, 6_000, NivelCobertura.Completo, "Cobertura de terremoto; responsabilidad civil; asistencia del hogar",
                Cob("Incendio y rayo", 10_000_000),
                Cob("Fenómenos naturales (huracán, inundación)", 10_000_000),
                Cob("Terremoto", 6_000_000),
                Cob("Responsabilidad civil familiar", 1_500_000)),
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
        var tipos = await _db.TiposSeguro.ToDictionaryAsync(t => t.Nombre, ct);
        var productos = await _db.ProductosSeguro
            .Include(p => p.TipoSeguro)
            .ToDictionaryAsync(p => p.Nombre, ct);

        var ahora = DateTime.UtcNow;
        var hoy = DateOnly.FromDateTime(ahora);
        var clave = _hasher.Hash(_credentials.ClientPassword);

        // Un directorio variado de clientes para que el portal no se vea vacío.
        var maria = ClienteDemo("María Pérez (demo)", "00100000001", "809-555-0201", "cliente1@demo.local", clave, ahora.AddDays(-120));
        var luis = ClienteDemo("Luis Gómez (demo)", "00100000002", "809-555-0202", "cliente2@demo.local", clave, ahora.AddDays(-110));
        var ana = ClienteDemo("Ana Rodríguez", "00100000003", "829-555-0203", "cliente3@demo.local", clave, ahora.AddDays(-95));
        var carlos = ClienteDemo("Carlos Martínez", "00100000004", "849-555-0204", "cliente4@demo.local", clave, ahora.AddDays(-90));
        var laura = ClienteDemo("Laura Fernández", "00100000005", "809-555-0205", "cliente5@demo.local", clave, ahora.AddDays(-85));
        var jose = ClienteDemo("José Ramírez", "00100000006", "829-555-0206", "cliente6@demo.local", clave, ahora.AddDays(-80));
        var carmen = ClienteDemo("Carmen Sánchez", "00100000007", "849-555-0207", "cliente7@demo.local", clave, ahora.AddDays(-70));
        var pedro = ClienteDemo("Pedro Castillo", "00100000008", "809-555-0208", "cliente8@demo.local", clave, ahora.AddDays(-65));
        var daniela = ClienteDemo("Daniela Torres", "00100000009", "829-555-0209", "cliente9@demo.local", clave, ahora.AddDays(-60));
        var miguel = ClienteDemo("Miguel Ángel Reyes", "00100000010", "849-555-0210", "cliente10@demo.local", clave, ahora.AddDays(-50));
        var sofia = ClienteDemo("Sofía Guzmán", "00100000011", "809-555-0211", "cliente11@demo.local", clave, ahora.AddDays(-45));
        var jorge = ClienteDemo("Jorge Rodríguez", "00100000012", "829-555-0212", "cliente12@demo.local", clave, ahora.AddDays(-42));
        var valeria = ClienteDemo("Valeria Jiménez", "00100000013", "849-555-0213", "cliente13@demo.local", clave, ahora.AddDays(-38));
        var ricardo = ClienteDemo("Ricardo Núñez", "00100000014", "809-555-0214", "cliente14@demo.local", clave, ahora.AddDays(-33));
        var elena = ClienteDemo("Elena Vargas", "00100000015", "829-555-0215", "cliente15@demo.local", clave, ahora.AddDays(-27));
        var andres = ClienteDemo("Andrés Peña", "00100000016", "849-555-0216", "cliente16@demo.local", clave, ahora.AddDays(-21));
        var clientes = new[] { maria, luis, ana, carlos, laura, jose, carmen, pedro, daniela, miguel, sofia, jorge, valeria, ricardo, elena, andres };

        // Próximas 12 jornadas laborables, con 7 franjas por día.
        var franjas = new[] { (8, 9), (9, 10), (10, 11), (11, 12), (14, 15), (15, 16), (16, 17) };
        var futuros = new List<HorarioDisponible>();
        for (var dia = hoy.AddDays(1); futuros.Count < 84; dia = dia.AddDays(1))
        {
            if (dia.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                continue;

            foreach (var (inicio, fin) in franjas)
                futuros.Add(HorarioDemo(asesor, dia, inicio, fin));
        }

        // Jornadas pasadas para asesorías que ya se realizaron.
        var pasados = new List<HorarioDisponible>();
        for (var dia = hoy.AddDays(-1); pasados.Count < 20; dia = dia.AddDays(-1))
        {
            if (dia.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                continue;

            foreach (var (inicio, fin) in franjas.Take(4))
                pasados.Add(HorarioDemo(asesor, dia, inicio, fin, disponible: false));
        }

        // Solicitudes pendientes: cada una reserva una franja futura distinta.
        var solicitudMaria = SolicitudDemo(maria, tipos["Vehículos"], futuros[0],
            "Necesito asegurar mi jeepeta; quiero comparar cobertura completa y asistencia en carretera.",
            3_000, EstadoSolicitudAsesoria.Pendiente, ahora.AddDays(-1).AddHours(-2));
        var solicitudAna = SolicitudDemo(ana, tipos["Salud"], futuros[8],
            "Busco un plan de salud familiar que incluya a mis dos hijos y consultas pediátricas.",
            4_500, EstadoSolicitudAsesoria.Pendiente, ahora.AddDays(-2).AddHours(-5));
        var solicitudCarlos = SolicitudDemo(carlos, tipos["Vida"], futuros[16],
            "Quiero proteger económicamente a mi familia y revisar opciones de ahorro.",
            2_500, EstadoSolicitudAsesoria.Pendiente, ahora.AddDays(-3).AddHours(-1));
        var solicitudLaura = SolicitudDemo(laura, tipos["Propiedad"], futuros[24],
            "Necesito asegurar mi vivienda en Santiago; me preocupan los huracanes y las inundaciones.",
            3_500, EstadoSolicitudAsesoria.Pendiente, ahora.AddDays(-4).AddHours(-6));
        var solicitudJose = SolicitudDemo(jose, tipos["Vehículos"], futuros[32],
            "Tengo una motocicleta y quiero la cobertura mínima exigida por la ley.",
            1_500, EstadoSolicitudAsesoria.Pendiente, ahora.AddDays(-5).AddHours(-3));
        var solicitudCarmen = SolicitudDemo(carmen, tipos["Salud"], futuros[40],
            "Quisiera un plan premium con odontología y cobertura en el exterior.",
            null, EstadoSolicitudAsesoria.Pendiente, ahora.AddDays(-6).AddHours(-4));

        // Solicitudes confirmadas: la cita está agendada y el asesor ya hizo el primer contacto.
        var solicitudDaniela = SolicitudDemo(daniela, tipos["Vehículos"], futuros[48],
            "Compré un vehículo nuevo y necesito cobertura todo riesgo desde la entrega.",
            5_000, EstadoSolicitudAsesoria.Confirmada, ahora.AddDays(-2).AddHours(-7));
        var solicitudMiguel = SolicitudDemo(miguel, tipos["Propiedad"], futuros[56],
            "Quiero asegurar el local de mi negocio y los equipos que utilizo.",
            null, EstadoSolicitudAsesoria.Confirmada, ahora.AddDays(-4).AddHours(-2));
        var solicitudSofia = SolicitudDemo(sofia, tipos["Salud"], futuros[64],
            "Busco un plan con red internacional porque viajo con frecuencia por trabajo.",
            8_000, EstadoSolicitudAsesoria.Confirmada, ahora.AddDays(-7).AddHours(-5));

        // Una solicitud cancelada para mostrar el ciclo de vida completo.
        var solicitudPedro = SolicitudDemo(pedro, tipos["Vida"], futuros[72],
            "Estaba evaluando un seguro de vida, pero decidió posponerlo por ahora.",
            1_800, EstadoSolicitudAsesoria.Cancelada, ahora.AddDays(-8).AddHours(-3));

        // Solicitudes atendidas: ya tienen asesoría registrada y cotizaciones asociadas.
        var solicitudJorge = SolicitudDemo(jorge, tipos["Vida"], pasados[0],
            "Quiero una protección de vida para mi esposa e hijos, con pagos mensuales cómodos.",
            3_000, EstadoSolicitudAsesoria.Atendida, FechaSolicitudPrevia(pasados[0], 8));
        var solicitudValeria = SolicitudDemo(valeria, tipos["Vehículos"], pasados[4],
            "Necesito cobertura completa para una jeepeta familiar.",
            4_500, EstadoSolicitudAsesoria.Atendida, FechaSolicitudPrevia(pasados[4], 10));
        var solicitudRicardo = SolicitudDemo(ricardo, tipos["Propiedad"], pasados[8],
            "Quiero asegurar mi casa y el contenido; vivo en una zona expuesta a huracanes.",
            2_800, EstadoSolicitudAsesoria.Atendida, FechaSolicitudPrevia(pasados[8], 6));
        var solicitudElena = SolicitudDemo(elena, tipos["Salud"], pasados[12],
            "Busco un plan de salud premium con cobertura internacional para mi familia.",
            7_000, EstadoSolicitudAsesoria.Atendida, FechaSolicitudPrevia(pasados[12], 12));
        var solicitudAndres = SolicitudDemo(andres, tipos["Vehículos"], pasados[16],
            "Quiero comparar opciones para asegurar mi carro sin exceder mi presupuesto.",
            3_500, EstadoSolicitudAsesoria.Atendida, FechaSolicitudPrevia(pasados[16], 5));

        var solicitudes = new[]
        {
            solicitudMaria, solicitudAna, solicitudCarlos, solicitudLaura, solicitudJose, solicitudCarmen,
            solicitudDaniela, solicitudMiguel, solicitudSofia, solicitudPedro,
            solicitudJorge, solicitudValeria, solicitudRicardo, solicitudElena, solicitudAndres
        };

        // Asesorías realizadas y sus cotizaciones con estados distintos.
        var asesoriaJorge = AsesoriaDemo(solicitudJorge,
            "Protección familiar con presupuesto mensual limitado; prefiere un plan completo con enfermedades graves.",
            "El cliente quedó en revisar la propuesta con su esposa.");
        var asesoriaValeria = AsesoriaDemo(solicitudValeria,
            "Cobertura todo riesgo para vehículo familiar; valora asistencia vial y auto de reemplazo.",
            "Se recomendó Auto Total por equilibrio entre prima y coberturas.");
        var asesoriaRicardo = AsesoriaDemo(solicitudRicardo,
            "Vivienda expuesta a huracanes; necesita protección de estructura, contenido y responsabilidad civil.",
            "El cliente solicitó una segunda cotización antes de decidir.");
        var asesoriaElena = AsesoriaDemo(solicitudElena,
            "Plan premium con red internacional y odontología para toda la familia.",
            "Pendiente de confirmación del cliente después de comparar deducibles.");
        var asesoriaAndres = AsesoriaDemo(solicitudAndres,
            "Presupuesto ajustado; busca la mejor relación entre prima, deducible y beneficios.",
            "Borrador listo para presentar en la próxima llamada.");

        var asesorias = new[] { asesoriaJorge, asesoriaValeria, asesoriaRicardo, asesoriaElena, asesoriaAndres };

        var cotizacionJorge = CotizacionDemo(asesoriaJorge, 1, EstadoCotizacion.Presentada,
            "Propuesta presentada con tres alternativas de protección de vida.",
            DetalleDemo(productos["Vida Integral"], 3_150, 91, true),
            DetalleDemo(productos["Vida Plus"], 2_350, 86, false),
            DetalleDemo(productos["Vida Protegida"], 1_650, 80, false),
            DetalleDemo(productos["Vida Temporal"], 900, 68, false));
        var cotizacionValeria = CotizacionDemo(asesoriaValeria, 1, EstadoCotizacion.Aceptada,
            "El cliente aceptó la alternativa recomendada y solicitó iniciar la emisión.",
            DetalleDemo(productos["Auto Total"], 4_300, 93, true),
            DetalleDemo(productos["Auto Ejecutivo"], 5_100, 89, false),
            DetalleDemo(productos["Auto Plus"], 2_500, 84, false),
            DetalleDemo(productos["Auto Básico (Ley)"], 1_200, 69, false));
        var cotizacionRicardo = CotizacionDemo(asesoriaRicardo, 1, EstadoCotizacion.Rechazada,
            "El cliente decidió posponer la contratación hasta la próxima renovación.",
            DetalleDemo(productos["Hogar Total"], 3_800, 90, true),
            DetalleDemo(productos["Hogar Premium"], 4_500, 87, false),
            DetalleDemo(productos["Hogar Seguro"], 2_300, 78, false),
            DetalleDemo(productos["Hogar Básico"], 1_100, 62, false));
        var cotizacionElena = CotizacionDemo(asesoriaElena, 1, EstadoCotizacion.Presentada,
            "Comparativo de planes de salud con énfasis en cobertura internacional.",
            DetalleDemo(productos["Salud Premium"], 7_500, 92, true),
            DetalleDemo(productos["Salud Global"], 9_200, 88, false),
            DetalleDemo(productos["Salud Familiar"], 3_900, 80, false),
            DetalleDemo(productos["Salud Esencial"], 1_800, 67, false));
        var cotizacionAndres = CotizacionDemo(asesoriaAndres, 1, EstadoCotizacion.Borrador,
            "Borrador por confirmar con el cliente en la próxima llamada.",
            DetalleDemo(productos["Auto Plus"], 2_700, 90, true),
            DetalleDemo(productos["Auto Total"], 4_300, 86, false),
            DetalleDemo(productos["Auto Ejecutivo"], 5_200, 82, false),
            DetalleDemo(productos["Auto Básico (Ley)"], 1_200, 68, false));

        var cotizaciones = new[] { cotizacionJorge, cotizacionValeria, cotizacionRicardo, cotizacionElena, cotizacionAndres };

        // Asistencias abiertas y resueltas, repartidas entre clientes y estados.
        var asistencias = new[]
        {
            AsistenciaDemo(luis, null, "No sé qué tipo de seguro me conviene; ¿pueden llamarme?",
                EstadoAsistencia.Pendiente, ahora.AddDays(-2).AddHours(-3)),
            AsistenciaDemo(maria, null, "Necesito una copia de la póliza y el recibo de pago.",
                EstadoAsistencia.Pendiente, ahora.AddDays(-1).AddHours(-6)),
            AsistenciaDemo(ana, asesor, "Quiero agregar a mi hijo recién nacido al plan de salud.",
                EstadoAsistencia.EnProceso, ahora.AddDays(-3).AddHours(-2)),
            AsistenciaDemo(carlos, null, "¿Cómo funciona el deducible en caso de una reclamación?",
                EstadoAsistencia.Pendiente, ahora.AddDays(-4).AddHours(-4)),
            AsistenciaDemo(laura, asesor, "Necesito actualizar mi correo y mi dirección de contacto.",
                EstadoAsistencia.EnProceso, ahora.AddDays(-5).AddHours(-1)),
            AsistenciaDemo(jose, null, "Quisiera saber si puedo cambiar la fecha de mi próxima cita.",
                EstadoAsistencia.Pendiente, ahora.AddDays(-6).AddHours(-5)),
            AsistenciaDemo(daniela, asesor, "Tengo una duda sobre el primer pago de la prima.",
                EstadoAsistencia.Resuelta, ahora.AddDays(-12).AddHours(-2)),
            AsistenciaDemo(sofia, asesor, "Solicito el recibo de la última cotización enviada.",
                EstadoAsistencia.Resuelta, ahora.AddDays(-9).AddHours(-7)),
        };

        _db.Clientes.AddRange(clientes);
        _db.Horarios.AddRange(futuros);
        _db.Horarios.AddRange(pasados);
        _db.SolicitudesAsesoria.AddRange(solicitudes);
        _db.SolicitudesAsistencia.AddRange(asistencias);
        _db.Asesorias.AddRange(asesorias);
        _db.Cotizaciones.AddRange(cotizaciones);
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

    private static Cliente ClienteDemo(
        string nombre, string documento, string telefono, string correo,
        string clave, DateTime fechaRegistro) => new()
        {
            Nombre = nombre,
            DocumentoIdentidad = documento,
            Telefono = telefono,
            Correo = correo,
            Contraseña = clave,
            FechaRegistro = fechaRegistro
        };

    private static HorarioDisponible HorarioDemo(
        Asesor asesor, DateOnly fecha, int inicio, int fin, bool disponible = true) => new()
        {
            Asesor = asesor,
            Fecha = fecha,
            HoraInicio = new TimeOnly(inicio, 0),
            HoraFin = new TimeOnly(fin, 0),
            Disponible = disponible
        };

    private static SolicitudAsesoria SolicitudDemo(
        Cliente cliente, TipoSeguro tipo, HorarioDisponible horario,
        string descripcion, decimal? presupuesto,
        EstadoSolicitudAsesoria estado, DateTime fechaSolicitud)
    {
        // Al cancelar se libera la franja, tal como hace el servicio de solicitudes.
        horario.Disponible = estado == EstadoSolicitudAsesoria.Cancelada;

        return new SolicitudAsesoria
        {
            Cliente = cliente,
            TipoSeguro = tipo,
            Horario = horario,
            DescripcionNecesidad = descripcion,
            Presupuesto = presupuesto,
            Estado = estado,
            FechaSolicitud = fechaSolicitud
        };
    }

    private static DateTime FechaSolicitudPrevia(HorarioDisponible horario, int diasAntes)
    {
        var citaUtc = horario.Fecha.ToDateTime(horario.HoraInicio).AddHours(4);
        return citaUtc.AddDays(-diasAntes);
    }

    private static Asesoria AsesoriaDemo(
        SolicitudAsesoria solicitud, string necesidades, string? notas)
    {
        solicitud.Estado = EstadoSolicitudAsesoria.Atendida;
        var horario = solicitud.Horario
            ?? throw new InvalidOperationException("La solicitud demo necesita un horario reservado.");

        return new Asesoria
        {
            Solicitud = solicitud,
            Cliente = solicitud.Cliente,
            Asesor = horario.Asesor,
            FechaAsesoria = horario.Fecha.ToDateTime(horario.HoraInicio).AddHours(4),
            NecesidadesIdentificadas = necesidades,
            Notas = notas
        };
    }

    private static Cotizacion CotizacionDemo(
        Asesoria asesoria, int diasDespues, EstadoCotizacion estado,
        string? observaciones, params DetalleCotizacion[] detalles)
    {
        var cotizacion = new Cotizacion
        {
            Asesoria = asesoria,
            FechaEmision = DateOnly.FromDateTime(asesoria.FechaAsesoria.AddDays(diasDespues)),
            Estado = estado,
            Observaciones = observaciones
        };

        foreach (var detalle in detalles)
        {
            detalle.Cotizacion = cotizacion;
            cotizacion.Detalles.Add(detalle);
        }

        return cotizacion;
    }

    private static DetalleCotizacion DetalleDemo(
        ProductoSeguro producto, decimal prima, decimal puntaje, bool recomendada) => new()
        {
            Producto = producto,
            PrimaCotizada = prima,
            PuntajeCoincidencia = puntaje,
            Recomendada = recomendada
        };

    private static SolicitudAsistencia AsistenciaDemo(
        Cliente cliente, Asesor? asesor, string motivo,
        EstadoAsistencia estado, DateTime fechaSolicitud) => new()
        {
            Cliente = cliente,
            Asesor = asesor,
            Motivo = motivo,
            Estado = estado,
            FechaSolicitud = fechaSolicitud
        };
}
