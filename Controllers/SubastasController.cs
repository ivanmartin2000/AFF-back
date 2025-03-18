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
    public class SubastasController : ControllerBase
    {
        private readonly AppDbContext _db;
        public SubastasController(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Muestra los productos en subasta que publicó el usuario logueado.
        /// Activo = true, FechaFin != null, FechaFin > ahora, IdUsuario = usuario logueado
        /// </summary>
        [HttpGet("publicados")]
        public async Task<IActionResult> GetProductosSubasta()
        {
            // Extraer IdUsuario del token
            if (!int.TryParse(User.FindFirst("IdUsuario")?.Value, out int idUsuario))
                return Unauthorized("No se pudo extraer el usuario.");

            // Consulta: Selecciona productos en subasta (Activo, FechaFin futura, IdUsuario)
            var productosSubasta = await _db.Productos
            .Where(p => p.Activo
                        && p.FechaFin.HasValue
                        && p.FechaFin.Value > DateTime.UtcNow
                        && p.IdUsuario == idUsuario)
            .Select(p => new
            {
                idProducto = p.IdProducto,
                nombre = p.Nombre ?? string.Empty,
                descripcion = p.Descripcion ?? string.Empty,
                precio = p.Precio,
                fechaFin = p.FechaFin,    // Se asume que no es null por el filtro HasValue
                rutaImagen = p.RutaImagen ?? string.Empty,
                nombreImagen = p.NombreImagen ?? string.Empty
            })
            .ToListAsync();


            return Ok(productosSubasta);
        }

        [HttpGet("por-usuario/{idUsuario}")]
        public async Task<IActionResult> GetProductosSubastaPorUsuario(int idUsuario)
        {
            // Consulta: Selecciona productos en subasta (Activo, FechaFin futura, IdUsuario)
            var productosSubasta = await _db.Productos
                .Where(p => p.Activo
                            && p.FechaFin.HasValue
                            && p.FechaFin.Value > DateTime.UtcNow
                            && p.IdUsuario == idUsuario)
                .Select(p => new
                {
                    idProducto = p.IdProducto,
                    nombre = p.Nombre ?? string.Empty,
                    descripcion = p.Descripcion ?? string.Empty,
                    precio = p.Precio,
                    fechaFin = p.FechaFin,    // Se asume que no es null por el filtro HasValue
                    rutaImagen = p.RutaImagen ?? string.Empty,
                    nombreImagen = p.NombreImagen ?? string.Empty
                })
                .ToListAsync();

            if (productosSubasta == null || productosSubasta.Count == 0)
            {
                return NotFound("No se encontraron productos en subasta para este usuario.");
            }

            return Ok(productosSubasta);
        }


        /// <summary>
        /// Cancela subastas que han expirado (FechaFin < ahora) y siguen Activo = true.
        /// Marca el producto como inactivo y limpia FechaFin.
        /// </summary>
        [HttpPost("cancelar-expiradas")]
        public async Task<IActionResult> CancelarSubastasExpiradas()
        {
            var ahora = DateTime.UtcNow;

            // Busca productos en subasta (FechaFin < ahora, Activo = true)
            var subastasExpiradas = await _db.Productos
                .Where(p => p.Activo
                            && p.FechaFin.HasValue
                            && p.FechaFin.Value < ahora)
                .ToListAsync();

            foreach (var prod in subastasExpiradas)
            {
                // Marca como cancelada
                prod.Activo = false;
                prod.FechaFin = null; // Limpia la fecha, o podrías usar un campo EstadoSubasta = "Cancelada"
            }

            await _db.SaveChangesAsync();
            return Ok(new { message = "Subastas expiradas canceladas correctamente." });
        }

        /// <summary>
        /// (Opcional) Muestra las subastas canceladas del usuario logueado.
        /// Productos que tengan Activo = false y FechaFin = null.
        /// </summary>
        [HttpGet("canceladas")]
        public async Task<IActionResult> GetSubastasCanceladas()
        {
            if (!int.TryParse(User.FindFirst("IdUsuario")?.Value, out int idUsuario))
                return Unauthorized("No se pudo extraer el usuario.");

            var subastasCanceladas = await _db.Productos
                .Where(p => !p.Activo
                            && !p.FechaFin.HasValue
                            && p.IdUsuario == idUsuario)
                .Select(p => new
                {
                    idProducto = p.IdProducto,
                    nombre = p.Nombre,
                    descripcion = p.Descripcion,
                    precio = p.Precio,
                    // Podrías guardar un campo EstadoSubasta = "Cancelada" si deseas
                    rutaImagen = p.RutaImagen,
                    nombreImagen = p.NombreImagen
                })
                .ToListAsync();

            return Ok(subastasCanceladas);
        }
    }
}
