using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sanalia.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "aseguradora",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, collation: "NOCASE"),
                    telefono = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    correo_contacto = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_aseguradora", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "asesor",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    correo = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, collation: "NOCASE"),
                    contraseña = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    rol = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    activo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asesor", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cliente",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    documento_identidad = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    telefono = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    correo = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, collation: "NOCASE"),
                    contraseña = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    fecha_registro = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cliente", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tipo_seguro",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    nombre = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, collation: "NOCASE"),
                    descripcion = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tipo_seguro", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "horario_disponible",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    asesor_id = table.Column<int>(type: "INTEGER", nullable: false),
                    fecha = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    hora_inicio = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    hora_fin = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    disponible = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_horario_disponible", x => x.id);
                    table.ForeignKey(
                        name: "fk_horario_disponible_asesor_asesor_id",
                        column: x => x.asesor_id,
                        principalTable: "asesor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "solicitud_asistencia",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    cliente_id = table.Column<int>(type: "INTEGER", nullable: false),
                    asesor_id = table.Column<int>(type: "INTEGER", nullable: true),
                    motivo = table.Column<string>(type: "TEXT", nullable: false),
                    estado = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    fecha_solicitud = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_solicitud_asistencia", x => x.id);
                    table.ForeignKey(
                        name: "fk_solicitud_asistencia_asesor_asesor_id",
                        column: x => x.asesor_id,
                        principalTable: "asesor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_solicitud_asistencia_cliente_cliente_id",
                        column: x => x.cliente_id,
                        principalTable: "cliente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "producto_seguro",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    aseguradora_id = table.Column<int>(type: "INTEGER", nullable: false),
                    tipo_id = table.Column<int>(type: "INTEGER", nullable: false),
                    nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    prima = table.Column<double>(type: "REAL", nullable: false),
                    deducible = table.Column<double>(type: "REAL", nullable: false),
                    nivel_cobertura = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    beneficios_adicionales = table.Column<string>(type: "TEXT", nullable: true),
                    vigente = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_producto_seguro", x => x.id);
                    table.ForeignKey(
                        name: "fk_producto_seguro_aseguradora_aseguradora_id",
                        column: x => x.aseguradora_id,
                        principalTable: "aseguradora",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_producto_seguro_tipos_seguro_tipo_id",
                        column: x => x.tipo_id,
                        principalTable: "tipo_seguro",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "solicitud_asesoria",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    cliente_id = table.Column<int>(type: "INTEGER", nullable: false),
                    tipo_id = table.Column<int>(type: "INTEGER", nullable: false),
                    descripcion_necesidad = table.Column<string>(type: "TEXT", nullable: true),
                    presupuesto = table.Column<double>(type: "REAL", nullable: true),
                    horario_id = table.Column<int>(type: "INTEGER", nullable: true),
                    estado = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    fecha_solicitud = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_solicitud_asesoria", x => x.id);
                    table.ForeignKey(
                        name: "fk_solicitud_asesoria_cliente_cliente_id",
                        column: x => x.cliente_id,
                        principalTable: "cliente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_solicitud_asesoria_horario_disponible_horario_id",
                        column: x => x.horario_id,
                        principalTable: "horario_disponible",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_solicitud_asesoria_tipos_seguro_tipo_id",
                        column: x => x.tipo_id,
                        principalTable: "tipo_seguro",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "cobertura",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    producto_id = table.Column<int>(type: "INTEGER", nullable: false),
                    descripcion = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    limite = table.Column<double>(type: "REAL", nullable: true),
                    condiciones = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cobertura", x => x.id);
                    table.ForeignKey(
                        name: "fk_cobertura_productos_seguro_producto_id",
                        column: x => x.producto_id,
                        principalTable: "producto_seguro",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "asesoria",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    solicitud_id = table.Column<int>(type: "INTEGER", nullable: false),
                    cliente_id = table.Column<int>(type: "INTEGER", nullable: false),
                    asesor_id = table.Column<int>(type: "INTEGER", nullable: false),
                    fecha_asesoria = table.Column<DateTime>(type: "TEXT", nullable: false),
                    necesidades_identificadas = table.Column<string>(type: "TEXT", nullable: true),
                    notas = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asesoria", x => x.id);
                    table.ForeignKey(
                        name: "fk_asesoria_asesor_asesor_id",
                        column: x => x.asesor_id,
                        principalTable: "asesor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_asesoria_cliente_cliente_id",
                        column: x => x.cliente_id,
                        principalTable: "cliente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_asesoria_solicitudes_asesoria_solicitud_id",
                        column: x => x.solicitud_id,
                        principalTable: "solicitud_asesoria",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "cotizacion",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    asesoria_id = table.Column<int>(type: "INTEGER", nullable: false),
                    fecha_emision = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    estado = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    observaciones = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cotizacion", x => x.id);
                    table.ForeignKey(
                        name: "fk_cotizacion_asesoria_asesoria_id",
                        column: x => x.asesoria_id,
                        principalTable: "asesoria",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "detalle_cotizacion",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    cotizacion_id = table.Column<int>(type: "INTEGER", nullable: false),
                    producto_id = table.Column<int>(type: "INTEGER", nullable: false),
                    prima_cotizada = table.Column<double>(type: "REAL", nullable: false),
                    puntaje_coincidencia = table.Column<double>(type: "REAL", nullable: true),
                    recomendada = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_detalle_cotizacion", x => x.id);
                    table.ForeignKey(
                        name: "fk_detalle_cotizacion_cotizacion_cotizacion_id",
                        column: x => x.cotizacion_id,
                        principalTable: "cotizacion",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_detalle_cotizacion_productos_seguro_producto_id",
                        column: x => x.producto_id,
                        principalTable: "producto_seguro",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_aseguradora_nombre",
                table: "aseguradora",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_asesor_correo",
                table: "asesor",
                column: "correo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_asesoria_asesor_id",
                table: "asesoria",
                column: "asesor_id");

            migrationBuilder.CreateIndex(
                name: "ix_asesoria_cliente_id_fecha_asesoria",
                table: "asesoria",
                columns: new[] { "cliente_id", "fecha_asesoria" });

            migrationBuilder.CreateIndex(
                name: "ix_asesoria_solicitud_id",
                table: "asesoria",
                column: "solicitud_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cliente_correo",
                table: "cliente",
                column: "correo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cliente_documento_identidad",
                table: "cliente",
                column: "documento_identidad",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cobertura_producto_id",
                table: "cobertura",
                column: "producto_id");

            migrationBuilder.CreateIndex(
                name: "ix_cotizacion_asesoria_id",
                table: "cotizacion",
                column: "asesoria_id");

            migrationBuilder.CreateIndex(
                name: "ix_detalle_cotizacion_cotizacion_id_producto_id",
                table: "detalle_cotizacion",
                columns: new[] { "cotizacion_id", "producto_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_detalle_cotizacion_producto_id",
                table: "detalle_cotizacion",
                column: "producto_id");

            migrationBuilder.CreateIndex(
                name: "ix_horario_disponible_asesor_id_fecha_hora_inicio",
                table: "horario_disponible",
                columns: new[] { "asesor_id", "fecha", "hora_inicio" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_producto_seguro_aseguradora_id",
                table: "producto_seguro",
                column: "aseguradora_id");

            migrationBuilder.CreateIndex(
                name: "ix_producto_seguro_tipo_id_vigente",
                table: "producto_seguro",
                columns: new[] { "tipo_id", "vigente" });

            migrationBuilder.CreateIndex(
                name: "ix_solicitud_asesoria_cliente_id",
                table: "solicitud_asesoria",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "ix_solicitud_asesoria_estado",
                table: "solicitud_asesoria",
                column: "estado");

            migrationBuilder.CreateIndex(
                name: "ix_solicitud_asesoria_horario_id",
                table: "solicitud_asesoria",
                column: "horario_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_solicitud_asesoria_tipo_id",
                table: "solicitud_asesoria",
                column: "tipo_id");

            migrationBuilder.CreateIndex(
                name: "ix_solicitud_asistencia_asesor_id",
                table: "solicitud_asistencia",
                column: "asesor_id");

            migrationBuilder.CreateIndex(
                name: "ix_solicitud_asistencia_cliente_id",
                table: "solicitud_asistencia",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "ix_solicitud_asistencia_estado",
                table: "solicitud_asistencia",
                column: "estado");

            migrationBuilder.CreateIndex(
                name: "ix_tipo_seguro_nombre",
                table: "tipo_seguro",
                column: "nombre",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cobertura");

            migrationBuilder.DropTable(
                name: "detalle_cotizacion");

            migrationBuilder.DropTable(
                name: "solicitud_asistencia");

            migrationBuilder.DropTable(
                name: "cotizacion");

            migrationBuilder.DropTable(
                name: "producto_seguro");

            migrationBuilder.DropTable(
                name: "asesoria");

            migrationBuilder.DropTable(
                name: "aseguradora");

            migrationBuilder.DropTable(
                name: "solicitud_asesoria");

            migrationBuilder.DropTable(
                name: "cliente");

            migrationBuilder.DropTable(
                name: "horario_disponible");

            migrationBuilder.DropTable(
                name: "tipo_seguro");

            migrationBuilder.DropTable(
                name: "asesor");
        }
    }
}
