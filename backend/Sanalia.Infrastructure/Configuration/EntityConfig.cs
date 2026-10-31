using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sanalia.Core.Entities;

namespace Sanalia.Infrastructure.Configuration;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> b)
    {
        b.ToTable("cliente");
        b.HasKey(x => x.Id);

        b.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
        b.Property(x => x.DocumentoIdentidad).HasMaxLength(20).IsRequired();
        b.Property(x => x.Telefono).HasMaxLength(20).IsRequired();
        b.Property(x => x.Correo).HasMaxLength(100).IsRequired().UseCollation("NOCASE");
        b.Property(x => x.Contraseña).HasMaxLength(255).IsRequired();

        b.HasIndex(x => x.DocumentoIdentidad).IsUnique();
        b.HasIndex(x => x.Correo).IsUnique();
    }

    public class AsesorConfiguration : IEntityTypeConfiguration<Asesor>
    {
        public void Configure(EntityTypeBuilder<Asesor> b)
        {
            b.ToTable("asesor");
            b.HasKey(x => x.Id);

            b.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
            b.Property(x => x.Correo).HasMaxLength(100).IsRequired().UseCollation("NOCASE");
            b.Property(x => x.Contraseña).HasMaxLength(255).IsRequired();
            b.Property(x => x.Rol).HasConversion<string>().HasMaxLength(20);

            b.HasIndex(x => x.Correo).IsUnique();
        }
    }

    public class AseguradoraConfiguration : IEntityTypeConfiguration<Aseguradora>
    {
        public void Configure(EntityTypeBuilder<Aseguradora> b)
        {
            b.ToTable("aseguradora");
            b.HasKey(x => x.Id);

            b.Property(x => x.Nombre).HasMaxLength(100).IsRequired().UseCollation("NOCASE");
            b.Property(x => x.Telefono).HasMaxLength(20);
            b.Property(x => x.CorreoContacto).HasMaxLength(100);

            b.HasIndex(x => x.Nombre).IsUnique();
        }
    }

    public class TipoSeguroConfiguration : IEntityTypeConfiguration<TipoSeguro>
    {
        public void Configure(EntityTypeBuilder<TipoSeguro> b)
        {
            b.ToTable("tipo_seguro");
            b.HasKey(x => x.Id);

            b.Property(x => x.Nombre).HasMaxLength(50).IsRequired().UseCollation("NOCASE");
            b.Property(x => x.Descripcion).HasMaxLength(200);

            b.HasIndex(x => x.Nombre).IsUnique();
        }
    }

    public class ProductoSeguroConfiguration : IEntityTypeConfiguration<ProductoSeguro>
    {
        public void Configure(EntityTypeBuilder<ProductoSeguro> b)
        {
            b.ToTable("producto_seguro");
            b.HasKey(x => x.Id);

            b.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
            b.Property(x => x.Prima).HasConversion<double>();
            b.Property(x => x.Deducible).HasConversion<double>();
            b.Property(x => x.NivelCobertura).HasConversion<string>().HasMaxLength(20);

            b.HasOne(x => x.Aseguradora)
                .WithMany(a => a.Productos)
                .HasForeignKey(x => x.AseguradoraId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.TipoSeguro)
                .WithMany(t => t.Productos)
                .HasForeignKey(x => x.TipoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Consulta frecuente del comparador: productos vigentes de un tipo.
            b.HasIndex(x => new { x.TipoId, x.Vigente });
        }
    }

    public class CoberturaConfiguration : IEntityTypeConfiguration<Cobertura>
    {
        public void Configure(EntityTypeBuilder<Cobertura> b)
        {
            b.ToTable("cobertura");
            b.HasKey(x => x.Id);

            b.Property(x => x.Descripcion).HasMaxLength(200).IsRequired();
            b.Property(x => x.Limite).HasConversion<double>();

            // Las coberturas pertenecen al producto: si se borra el producto, se borran con él.
            b.HasOne(x => x.Producto)
                .WithMany(p => p.Coberturas)
                .HasForeignKey(x => x.ProductoId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class HorarioDisponibleConfiguration : IEntityTypeConfiguration<HorarioDisponible>
    {
        public void Configure(EntityTypeBuilder<HorarioDisponible> b)
        {
            b.ToTable("horario_disponible");
            b.HasKey(x => x.Id);

            b.HasOne(x => x.Asesor)
                .WithMany(a => a.Horarios)
                .HasForeignKey(x => x.AsesorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Un asesor no puede tener dos franjas que empiecen a la misma hora el mismo día.
            b.HasIndex(x => new { x.AsesorId, x.Fecha, x.HoraInicio }).IsUnique();
        }
    }

    public class SolicitudAsesoriaConfiguration : IEntityTypeConfiguration<SolicitudAsesoria>
    {
        public void Configure(EntityTypeBuilder<SolicitudAsesoria> b)
        {
            b.ToTable("solicitud_asesoria");
            b.HasKey(x => x.Id);

            b.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20);
            b.Property(x => x.Presupuesto).HasConversion<double>();

            b.HasOne(x => x.Cliente)
                .WithMany(c => c.SolicitudesAsesoria)
                .HasForeignKey(x => x.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.TipoSeguro)
                .WithMany(t => t.Solicitudes)
                .HasForeignKey(x => x.TipoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación 1 a 1 opcional: EF crea un índice único sobre id_horario, así que la base de
            // datos impide que dos solicitudes reserven el mismo horario (los NULL no cuentan).
            b.HasOne(x => x.Horario)
                .WithOne(h => h.Solicitud)
                .HasForeignKey<SolicitudAsesoria>(x => x.HorarioId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => x.Estado);
        }
    }

    public class SolicitudAsistenciaConfiguration : IEntityTypeConfiguration<SolicitudAsistencia>
    {
        public void Configure(EntityTypeBuilder<SolicitudAsistencia> b)
        {
            b.ToTable("solicitud_asistencia");
            b.HasKey(x => x.Id);

            b.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20);

            b.HasOne(x => x.Cliente)
                .WithMany(c => c.SolicitudesAsistencia)
                .HasForeignKey(x => x.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Asesor)
                .WithMany(a => a.Asistencias)
                .HasForeignKey(x => x.AsesorId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => x.Estado);
        }
    }

    public class AsesoriaConfiguration : IEntityTypeConfiguration<Asesoria>
    {
        public void Configure(EntityTypeBuilder<Asesoria> b)
        {
            b.ToTable("asesoria");
            b.HasKey(x => x.Id);

            // Una solicitud origina como máximo una asesoría (índice único sobre id_solicitud).
            b.HasOne(x => x.Solicitud)
                .WithOne(s => s.Asesoria)
                .HasForeignKey<Asesoria>(x => x.SolicitudId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Cliente)
                .WithMany(c => c.Asesorias)
                .HasForeignKey(x => x.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Asesor)
                .WithMany(a => a.Asesorias)
                .HasForeignKey(x => x.AsesorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Para el historial por cliente (proceso P8).
            b.HasIndex(x => new { x.ClienteId, x.FechaAsesoria });
        }
    }

    public class CotizacionConfiguration : IEntityTypeConfiguration<Cotizacion>
    {
        public void Configure(EntityTypeBuilder<Cotizacion> b)
        {
            b.ToTable("cotizacion");
            b.HasKey(x => x.Id);

            b.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20);

            b.HasOne(x => x.Asesoria)
                .WithMany(a => a.Cotizaciones)
                .HasForeignKey(x => x.AsesoriaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class DetalleCotizacionConfiguration : IEntityTypeConfiguration<DetalleCotizacion>
    {
        public void Configure(EntityTypeBuilder<DetalleCotizacion> b)
        {
            b.ToTable("detalle_cotizacion");
            b.HasKey(x => x.Id);

            b.Property(x => x.PrimaCotizada).HasConversion<double>();
            b.Property(x => x.PuntajeCoincidencia).HasConversion<double>();

            // Los detalles pertenecen a la cotización.
            b.HasOne(x => x.Cotizacion)
                .WithMany(c => c.Detalles)
                .HasForeignKey(x => x.CotizacionId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Producto)
                .WithMany(p => p.Detalles)
                .HasForeignKey(x => x.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            // El mismo producto no se repite dentro de una cotización.
            b.HasIndex(x => new { x.CotizacionId, x.ProductoId }).IsUnique();
        }
    }
}