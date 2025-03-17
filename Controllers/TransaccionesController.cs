using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AFF_back;

namespace AFF_back.Controllers
{
    [ApiController]
    [Route("api/transacciones")]
    public class TransaccionesController : ControllerBase
    {
        private readonly AppDbContext _db;
        public TransaccionesController(AppDbContext db)
        {
            _db = db;
        }

        // Endpoint para obtener subastas activas (productos en subasta)
        [HttpGet("subastas-activas")]
        public async Task<IActionResult> GetSubastasActivas()
        {
            var subastas = await _db.Productos
                .Where(p => p.Activo
                            && p.TipoPublicacion == "subasta"
                            && p.FechaFin.HasValue
                            && p.FechaFin.Value > DateTime.UtcNow)
                .Select(p => new
                {
                    p.IdProducto,
                    p.Nombre,
                    Descripcion = p.Descripcion ?? string.Empty,
                    p.Precio,
                    p.FechaFin,
                    RutaImagen = p.RutaImagen ?? string.Empty,
                    NombreImagen = p.NombreImagen ?? string.Empty
                })
                .ToListAsync();

            return Ok(subastas);
        }

        // Endpoint para obtener productos activos (en venta)
        [HttpGet("productos-activos")]
        public async Task<IActionResult> GetProductosActivos()
        {
            var productos = await _db.Productos
                .Where(p => p.Activo
                            && p.TipoPublicacion == "venta")
                .Select(p => new
                {
                    p.IdProducto,
                    p.Nombre,
                    Descripcion = p.Descripcion ?? string.Empty,
                    p.Precio,
                    p.FechaRegistro,
                    RutaImagen = p.RutaImagen ?? string.Empty,
                    NombreImagen = p.NombreImagen ?? string.Empty
                })
                .ToListAsync();

            return Ok(productos);
        }
    }
}
