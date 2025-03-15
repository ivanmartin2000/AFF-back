using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AFF_back;
using System.Security.Claims;

namespace AFF_back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PerfilController : ControllerBase
    {
        private readonly AppDbContext _db;
        public PerfilController(AppDbContext db)
        {
            _db = db;
        }

        [Authorize]
        [HttpGet("datos-usuario")]
        public async Task<IActionResult> GetDatosUsuario()
        {
            // Extraer el IdUsuario del token
            if (!int.TryParse(User.FindFirst("IdUsuario")?.Value, out int idUsuario))
                return Unauthorized("No se pudo extraer el usuario.");

            // Traer datos básicos del usuario
            var usuario = await _db.Usuarios
                .Where(u => u.IdUsuario == idUsuario)
                .Select(u => new {
                    u.IdUsuario,
                    NombreCompleto = u.Nombres + " " + u.Apellidos,
                    u.Correo,
                    u.ImagenPerfil,
                    u.Descripcion
                })
                .FirstOrDefaultAsync();

            if (usuario == null)
                return NotFound("Usuario no encontrado.");

            // Traer direcciones
            var direcciones = await _db.DireccionesUsuario
                .Where(d => d.IdUsuario == idUsuario)
                .Select(d => new {
                    d.IdDireccion,
                    d.Calle,
                    d.Numero,
                    d.IdDistrito,
                    // Podrías incluir joins para traer la descripción del Distrito, Provincia, etc.
                    // Ejemplo: d.Distrito.Descripcion, d.Distrito.Provincia.Descripcion, etc.
                })
                .ToListAsync();

            // Traer tarjetas
            var tarjetas = await _db.Tarjetas
                .Where(t => t.IdUsuario == idUsuario)
                .Select(t => new {
                    t.IdTarjeta,
                    t.NumeroTarjeta,
                    t.Titular,
                    t.FechaExpiracion,
                    t.TipoTarjeta
                })
                .ToListAsync();

            return Ok(new
            {
                Usuario = usuario,
                Direcciones = direcciones,
                Tarjetas = tarjetas
            });
        }
    }
}
