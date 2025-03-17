using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using AFF_back; // Ajusta el namespace según tu proyecto

namespace AFF_back.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _db;

        public DashboardController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("metrics")]
        public async Task<IActionResult> GetDashboardMetrics()
        {
            // 1) Mejor Vendedor (global)
            // Se asume que "vendedor" es el dueño del producto (PRODUCTO.IdUsuario)
            // y que se vincula con VENTA -> DETALLE_VENTA -> PRODUCTO para saber quién vendió
            var mejorVendedor = await _db.DetalleVentas
                .Include(dv => dv.Venta)
                .Include(dv => dv.Producto)
                .Where(dv => dv.Venta != null && dv.Producto != null)
                .GroupBy(dv => dv.Producto.IdUsuario)  // Agrupa por vendedor
                .Select(g => new {
                    IdVendedor = g.Key,
                    // Se asume que el total de la venta se reparte en DETALLE_VENTA,
                    // usualmente se multiplica por la cantidad, etc.
                    // Aquí simplificamos usando dv.Total
                    MontoVendido = g.Sum(dv => dv.Total)
                })
                .OrderByDescending(x => x.MontoVendido)
                .FirstOrDefaultAsync();

            // Extrae datos para mostrar (puedes unir con USUARIO para tener nombres)
            var nombreVendedor = "";
            if (mejorVendedor != null)
            {
                var vendedor = await _db.Usuarios
                    .Where(u => u.IdUsuario == mejorVendedor.IdVendedor)
                    .Select(u => u.Nombres + " " + u.Apellidos)
                    .FirstOrDefaultAsync();

                nombreVendedor = vendedor ?? "Vendedor Desconocido";
            }

            // 2) Mejor Comprador (global)
            // "comprador" = VENTA.IdCliente
            var mejorComprador = await _db.Ventas
                .GroupBy(v => v.IdCliente)
                .Select(g => new {
                    IdCliente = g.Key,
                    MontoComprado = g.Sum(x => x.MontoTotal)
                })
                .OrderByDescending(x => x.MontoComprado)
                .FirstOrDefaultAsync();

            var nombreComprador = "";
            if (mejorComprador != null)
            {
                var comprador = await _db.Usuarios
                    .Where(u => u.IdUsuario == mejorComprador.IdCliente)
                    .Select(u => u.Nombres + " " + u.Apellidos)
                    .FirstOrDefaultAsync();
                nombreComprador = comprador ?? "Comprador Desconocido";
            }

            // 3) Ranking Subastas (por precio final o pujas)
            // Si tu lógica es: PRODUCTO.TipoPublicacion == "subasta", 
            // y el precio final es la puja ganadora (o dv.Total en la venta final).
            // Como ejemplo, usaremos la sumatoria del precio final (dv.Total).
            // Realmente depende de cómo registres subastas finalizadas.
            var rankingSubastas = await _db.DetalleVentas
                .Include(dv => dv.Producto)
                .Where(dv => dv.Producto != null && dv.Producto.TipoPublicacion == "subasta")
                .GroupBy(dv => dv.Producto.IdProducto)
                .Select(g => new {
                    IdProducto = g.Key,
                    PrecioFinal = g.Sum(dv => dv.Total)  // asumiendo 1 DETALLE_VENTA por subasta final
                })
                .OrderByDescending(x => x.PrecioFinal)
                .Take(5) // top 5 subastas
                .ToListAsync();

            // 4) Ventas totales mensuales
            // Se agrupan las ventas por mes/año y se suman MontoTotal
            var startOf2025 = new DateTime(2025, 1, 1);
            var monthlyVentas = await _db.Ventas
                .Where(v => v.FechaVenta >= startOf2025)
                .GroupBy(v => new { v.FechaVenta.Value.Year, v.FechaVenta.Value.Month })
                .Select(g => new {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Total = g.Sum(x => x.MontoTotal)
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            var monthlyVentasResult = monthlyVentas.Select(item => new {
                monthLabel = new DateTime(item.Year, item.Month, 1)
                    .ToString("MMM yyyy", CultureInfo.CurrentCulture),
                total = item.Total
            });

            // 5) Productos vendidos por mes
            var monthlyProductos = await _db.DetalleVentas
                .Include(dv => dv.Venta)
                .Where(dv => dv.Venta.FechaVenta >= startOf2025)
                .GroupBy(dv => new { dv.Venta.FechaVenta.Value.Year, dv.Venta.FechaVenta.Value.Month })
                .Select(g => new {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Cantidad = g.Sum(x => x.Cantidad)
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            var monthlyProductosResult = monthlyProductos.Select(item => new {
                monthLabel = new DateTime(item.Year, item.Month, 1)
                    .ToString("MMM yyyy", CultureInfo.CurrentCulture),
                total = item.Cantidad
            });

            // 6) Subastas finalizadas por mes
            // Asumimos que "subasta finalizada" = DETALLE_VENTA con PRODUCTO.TipoPublicacion = "subasta"
            var monthlySubastas = await _db.DetalleVentas
                .Include(dv => dv.Venta)
                .Include(dv => dv.Producto)
                .Where(dv => dv.Venta.FechaVenta >= startOf2025
                    && dv.Producto.TipoPublicacion == "subasta")
                .GroupBy(dv => new { dv.Venta.FechaVenta.Value.Year, dv.Venta.FechaVenta.Value.Month })
                .Select(g => new {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    CountSubastas = g.Count()
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            var monthlySubastasResult = monthlySubastas.Select(item => new {
                monthLabel = new DateTime(item.Year, item.Month, 1)
                    .ToString("MMM yyyy", CultureInfo.CurrentCulture),
                total = item.CountSubastas
            });

            // Retornar todo en un solo objeto
            return Ok(new
            {
                metrics = new
                {
                    // métricas que ya tenías, p.ej.:
                    // VentasTotales, SubastasActivas, etc.
                    MejorVendedor = new
                    {
                        IdUsuario = mejorVendedor?.IdVendedor,
                        Nombre = nombreVendedor,
                        Monto = mejorVendedor?.MontoVendido ?? 0
                    },
                    MejorComprador = new
                    {
                        IdUsuario = mejorComprador?.IdCliente,
                        Nombre = nombreComprador,
                        Monto = mejorComprador?.MontoComprado ?? 0
                    },
                    // Ejemplo: top 5 subastas con mayor precio final
                    RankingSubastas = rankingSubastas
                },
                monthlyVentas = monthlyVentasResult,
                monthlyProductos = monthlyProductosResult,
                monthlySubastas = monthlySubastasResult
            });
        }
    }
}
