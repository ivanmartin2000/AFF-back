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
    public class EnviosController : ControllerBase
    {
        private readonly AppDbContext _db;

        public EnviosController(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Muestra los envíos que el usuario espera recibir (el usuario es el comprador en la venta).
        /// </summary>
        [HttpGet("recibidos")]
        public async Task<IActionResult> GetEnviosRecibidos()
        {
            // Obtener el id del usuario logueado
            if (!int.TryParse(User.FindFirst("IdUsuario")?.Value, out int idUsuario))
                return Unauthorized("No se pudo extraer el usuario.");

            // Unir ENVIO con VENTA para filtrar donde el comprador (IdCliente) sea el usuario actual
            var enviosRecibidos = await (
                from e in _db.Envios
                join v in _db.Ventas on e.IdVenta equals v.IdVenta
                where v.IdCliente == idUsuario
                select new
                {
                    e.IdEnvio,
                    e.IdVenta,
                    e.EstadoEnvio,
                    e.FechaEnvio,
                    e.TrackingNumber,
                    e.FechaRegistro,
                    // Datos opcionales de la venta
                    v.MontoTotal,
                    v.FechaVenta
                }
            ).ToListAsync();

            return Ok(enviosRecibidos);
        }

        /// <summary>
        /// Muestra los envíos que el usuario ha realizado (el usuario es el vendedor).
        /// </summary>
        [HttpGet("realizados")]
        public async Task<IActionResult> GetEnviosRealizados()
        {
            // Obtener el id del usuario logueado
            if (!int.TryParse(User.FindFirst("IdUsuario")?.Value, out int idUsuario))
                return Unauthorized("No se pudo extraer el usuario.");

            // Para saber quién es el vendedor, unimos ENVIO -> VENTA -> DETALLE_VENTA -> PRODUCTO 
            // y filtramos por p.IdUsuario == idUsuario (el vendedor).
            // O si en tu esquema la venta ya almacena IdUsuarioVendedor, usarías ese campo directamente.
            var enviosRealizados = await (
                from e in _db.Envios
                join v in _db.Ventas on e.IdVenta equals v.IdVenta
                join dv in _db.DetalleVentas on v.IdVenta equals dv.IdVenta
                join p in _db.Productos on dv.IdProducto equals p.IdProducto
                where p.IdUsuario == idUsuario
                select new
                {
                    e.IdEnvio,
                    e.IdVenta,
                    e.EstadoEnvio,
                    e.FechaEnvio,
                    e.TrackingNumber,
                    e.FechaRegistro,
                    // Datos opcionales de la venta o producto
                    v.MontoTotal,
                    v.FechaVenta,
                    producto = p.Nombre
                }
            ).ToListAsync();

            return Ok(enviosRealizados);
        }
    }
}
