using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AFF_back.Migrations
{
    /// <inheritdoc />
    public partial class ActualizacionEnvioPuja : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PUJA",
                columns: table => new
                {
                    IdPuja = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdSubasta = table.Column<int>(type: "int", nullable: true),
                    IdUsuario = table.Column<int>(type: "int", nullable: true),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FechaPuja = table.Column<DateTime>(type: "datetime", nullable: true),
                    IdUsuarioComprador = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PUJA", x => x.IdPuja);
                    table.ForeignKey(
                        name: "FK_PUJA_USUARIO_IdUsuarioComprador",
                        column: x => x.IdUsuarioComprador,
                        principalTable: "USUARIO",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VENTA",
                columns: table => new
                {
                    IdVenta = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCliente = table.Column<int>(type: "int", nullable: false),
                    FechaVenta = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalProducto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MontoTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Contacto = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdDistrito = table.Column<int>(type: "int", nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Direccion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdTransaccion = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VENTA", x => x.IdVenta);
                    table.ForeignKey(
                        name: "FK_VENTA_USUARIO_IdCliente",
                        column: x => x.IdCliente,
                        principalTable: "USUARIO",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DETALLE_VENTA",
                columns: table => new
                {
                    IdDetalleVenta = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdVenta = table.Column<int>(type: "int", nullable: false),
                    IdProducto = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DETALLE_VENTA", x => x.IdDetalleVenta);
                    table.ForeignKey(
                        name: "FK_DETALLE_VENTA_PRODUCTO_IdProducto",
                        column: x => x.IdProducto,
                        principalTable: "PRODUCTO",
                        principalColumn: "IdProducto",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DETALLE_VENTA_VENTA_IdVenta",
                        column: x => x.IdVenta,
                        principalTable: "VENTA",
                        principalColumn: "IdVenta",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ENVIO",
                columns: table => new
                {
                    IdEnvio = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdVenta = table.Column<int>(type: "int", nullable: false),
                    EstadoEnvio = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaEnvio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrackingNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ENVIO", x => x.IdEnvio);
                    table.ForeignKey(
                        name: "FK_ENVIO_VENTA_IdVenta",
                        column: x => x.IdVenta,
                        principalTable: "VENTA",
                        principalColumn: "IdVenta",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DETALLE_VENTA_IdProducto",
                table: "DETALLE_VENTA",
                column: "IdProducto");

            migrationBuilder.CreateIndex(
                name: "IX_DETALLE_VENTA_IdVenta",
                table: "DETALLE_VENTA",
                column: "IdVenta");

            migrationBuilder.CreateIndex(
                name: "IX_ENVIO_IdVenta",
                table: "ENVIO",
                column: "IdVenta",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PUJA_IdUsuarioComprador",
                table: "PUJA",
                column: "IdUsuarioComprador");

            migrationBuilder.CreateIndex(
                name: "IX_VENTA_IdCliente",
                table: "VENTA",
                column: "IdCliente");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DETALLE_VENTA");

            migrationBuilder.DropTable(
                name: "ENVIO");

            migrationBuilder.DropTable(
                name: "PUJA");

            migrationBuilder.DropTable(
                name: "VENTA");
        }
    }
}
