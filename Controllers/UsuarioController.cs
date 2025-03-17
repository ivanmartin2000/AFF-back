using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AFF_back;
using System.Security.Claims;

namespace AFF_back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _db;

        public UsuariosController(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Obtiene los datos privados del usuario logueado, como direcciones y tarjetas.
        /// </summary>
        [Authorize]
        [HttpGet("datos-usuario")]
        public async Task<IActionResult> GetDatosUsuario()
        {
            // Extraer el IdUsuario del token
            if (!int.TryParse(User.FindFirst("IdUsuario")?.Value, out int idUsuario))
                return Unauthorized("No se pudo extraer el usuario del token.");

            // Datos básicos del usuario
            var usuario = await _db.Usuarios
                .Where(u => u.IdUsuario == idUsuario)
                .Select(u => new
                {
                    u.IdUsuario,
                    NombreCompleto = (u.Nombres + " " + u.Apellidos).Trim(),
                    Correo = u.Correo ?? string.Empty,
                    ImagenPerfil = u.ImagenPerfil ?? string.Empty,
                    Descripcion = u.Descripcion ?? string.Empty
                })
                .FirstOrDefaultAsync();

            if (usuario == null)
                return NotFound("Usuario no encontrado.");

            // Obtener direcciones
            var direcciones = await _db.DireccionesUsuario
                .Where(d => d.IdUsuario == idUsuario)
                .Select(d => new
                {
                    d.IdDireccion,
                    d.Calle,
                    d.Numero,
                    d.IdDistrito
                    // Podrías incluir joins para traer la descripción del Distrito/Provincia si lo deseas
                })
                .ToListAsync();

            // Obtener tarjetas
            var tarjetas = await _db.Tarjetas
                .Where(t => t.IdUsuario == idUsuario)
                .Select(t => new
                {
                    t.IdTarjeta,
                    NumeroTarjeta = t.NumeroTarjeta ?? string.Empty,
                    Titular = t.Titular ?? string.Empty,
                    t.FechaExpiracion,
                    TipoTarjeta = t.TipoTarjeta ?? string.Empty
                })
                .ToListAsync();

            return Ok(new
            {
                Usuario = usuario,
                Direcciones = direcciones,
                Tarjetas = tarjetas
            });
        }

        /// <summary>
        /// Obtiene el perfil público de un usuario por su Id.
        /// Incluye datos básicos, subastas activas y ventas activas.
        /// </summary>
        [HttpGet("perfil-publico/{id}")]
        public async Task<IActionResult> GetPerfilPublico(int id)
        {
            // Datos básicos del usuario
            var usuario = await _db.Usuarios
                .Where(u => u.IdUsuario == id)
                .Select(u => new
                {
                    u.IdUsuario,
                    NombreCompleto = (u.Nombres + " " + u.Apellidos).Trim(),
                    Correo = u.Correo ?? string.Empty,
                    ImagenPerfil = u.ImagenPerfil ?? string.Empty,
                    Descripcion = u.Descripcion ?? string.Empty
                })
                .FirstOrDefaultAsync();

            if (usuario == null)
                return NotFound("Usuario no encontrado.");

            // Subastas activas: productos activos con FechaFin definida y futura
            var subastas = await _db.Productos
                .Where(p => p.IdUsuario == id
                            && p.Activo
                            && p.FechaFin.HasValue
                            && p.FechaFin.Value > DateTime.UtcNow)
                .Select(p => new
                {
                    p.IdProducto,
                    Nombre = p.Nombre ?? string.Empty,
                    Descripcion = p.Descripcion ?? string.Empty,
                    p.Precio,
                    p.FechaFin,
                    RutaImagen = p.RutaImagen ?? string.Empty,
                    NombreImagen = p.NombreImagen ?? string.Empty
                })
                .ToListAsync();

            // Ventas activas: productos activos sin FechaFin (no en subasta)
            var ventas = await _db.Productos
                .Where(p => p.IdUsuario == id
                            && p.Activo
                            && !p.FechaFin.HasValue)
                .Select(p => new
                {
                    p.IdProducto,
                    Nombre = p.Nombre ?? string.Empty,
                    Descripcion = p.Descripcion ?? string.Empty,
                    p.Precio,
                    p.FechaRegistro,
                    RutaImagen = p.RutaImagen ?? string.Empty,
                    NombreImagen = p.NombreImagen ?? string.Empty
                })
                .ToListAsync();

            return Ok(new
            {
                Usuario = usuario,
                Subastas = subastas,
                Ventas = ventas
            });
        }
    }
}
