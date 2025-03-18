using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AFF_back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LayoutController : ControllerBase
    {
        private readonly AppDbContext _db;

        public LayoutController(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Retorna datos básicos del usuario logueado (imagen de perfil, nombre, etc.).
        /// Ej: GET /api/layout/usuario
        /// </summary>
        [HttpGet("usuario")]
        [Authorize]
        public async Task<IActionResult> GetUsuarioLayout()
        {
            if (!int.TryParse(User.FindFirst("IdUsuario")?.Value, out int idUsuario))
                return Unauthorized("No se pudo extraer el usuario.");

            var usuario = await _db.Usuarios
                .Where(u => u.IdUsuario == idUsuario)
                .Select(u => new {
                    u.IdUsuario,
                    NombreCompleto = u.Nombres + " " + u.Apellidos,
                    ImagenPerfil = u.ImagenPerfil
                })
                .FirstOrDefaultAsync();

            if (usuario == null)
                return NotFound("Usuario no encontrado.");

            return Ok(usuario);
        }

        /// <summary>
        /// Permite buscar usuarios y/o productos por nombre.
        /// Ej: GET /api/layout/buscar?query=algunTexto
        /// </summary>
        [HttpGet("buscar")]
        public async Task<IActionResult> Buscar([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Debe proveer un término de búsqueda.");

            // 1) Buscar usuarios
            var usuarios = await _db.Usuarios
                .Where(u => u.Nombres.Contains(query) || u.Apellidos.Contains(query))
                .Select(u => new {
                    Tipo = "usuario",
                    Id = u.IdUsuario,
                    Nombre = u.Nombres + " " + u.Apellidos,
                    Imagen = u.ImagenPerfil
                })
                .ToListAsync();

            // 2) Buscar productos
            var productos = await _db.Productos
                .Where(p => p.Nombre.Contains(query) && p.Activo)
                .Select(p => new {
                    Tipo = "producto",
                    Id = p.IdProducto,
                    Nombre = p.Nombre,
                    Precio = p.Precio,
                    Imagen = (p.RutaImagen ?? "") + (p.NombreImagen ?? "")
                })
                .ToListAsync();

            // Combinar
            var resultados = usuarios
                .Concat<object>(productos)
                .ToList();

            return Ok(resultados);
        }
    }
}
