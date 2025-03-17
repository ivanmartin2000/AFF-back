using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AFF_back;
using System.Security.Claims;

namespace AFF_back.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ComprasController : ControllerBase
    {
        private readonly AppDbContext _db;
        public ComprasController(AppDbContext db)
        {
            _db = db;
        }
        [Authorize]
        [HttpGet("productos-comprados")]
        public async Task<IActionResult> GetProductosComprados()
        {
            if (!int.TryParse(User.FindFirst("IdUsuario")?.Value, out int idUsuario))
                return Unauthorized("No se pudo extraer el usuario.");

            // Aquí se asume que IdCliente en VENTA representa al comprador.
            var productosComprados = await (
                from venta in _db.Ventas
                where venta.IdCliente == idUsuario
                join detalle in _db.DetalleVentas on venta.IdVenta equals detalle.IdVenta
                join producto in _db.Productos on detalle.IdProducto equals producto.IdProducto
                select new
                {
                    id = producto.IdProducto,
                    nombre = producto.Nombre,
                    fechaCompra = venta.FechaVenta,
                    precio = detalle.Total
                }
            ).ToListAsync();

            return Ok(productosComprados);
        }

    }
}
