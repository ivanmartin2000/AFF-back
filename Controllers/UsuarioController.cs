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
        /// Obtiene el ID del usuario autenticado desde el token JWT.
        /// </summary>
        [Authorize]
        [HttpGet("mi-id")]
        public async Task<IActionResult> GetUserIdByEmail()
        {
            try
            {
                // Obtener el correo electrónico del usuario desde el token
                var userEmailClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userEmailClaim == null)
                {
                    return Unauthorized(new { message = "No se encontró el correo electrónico en el token." });
                }

                string userEmail = userEmailClaim.Value; // Obtener el correo electrónico del claim

                // Buscar el usuario en la base de datos por su correo electrónico
                var usuario = await _db.Usuarios
                    .Where(u => u.Correo == userEmail)
                    .FirstOrDefaultAsync();

                if (usuario == null)
                {
                    return NotFound(new { message = "Usuario no encontrado." });
                }

                // Devolver el ID del usuario
                return Ok(usuario.IdUsuario);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener el ID del usuario", error = ex.Message });
            }
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

        // Endpoint para obtener el perfil del usuario
        [HttpGet("perfil-usuario/{id}")]
        public async Task<IActionResult> GetPerfilUsuario(int id)
        {
            var usuario = await _db.Usuarios
                .Where(u => u.IdUsuario == id)
                .Select(u => new
                {
                    u.IdUsuario,
                    u.Nombres,
                    u.Apellidos,
                    u.Correo,
                    u.ImagenPerfil,
                    u.Descripcion
                })
                .FirstOrDefaultAsync();

            if (usuario == null)
                return NotFound("Usuario no encontrado.");

            var direcciones = await _db.DireccionesUsuario
                .Where(d => d.IdUsuario == id)
                .Select(d => new {
                    d.IdDireccion,
                    d.Calle,
                    d.Numero,
                    d.IdDistrito,
                    label = $"{d.Calle} {d.Numero}, {d.IdDistrito}"  // Agregamos la propiedad 'label' para que coincida con el frontend
                })
                .ToListAsync();

            var tarjetas = await _db.Tarjetas
                .Where(t => t.IdUsuario == id)
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
