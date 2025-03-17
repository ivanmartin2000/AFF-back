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

        // Endpoint para obtener los datos privados del usuario logueado
        [Authorize]
        [HttpGet("datos-usuario")]
        public async Task<IActionResult> GetDatosUsuario()
        {
            // Extraer el IdUsuario del token
            if (!int.TryParse(User.FindFirst("IdUsuario")?.Value, out int idUsuario))
                return Unauthorized("No se pudo extraer el usuario.");

            // Datos básicos del usuario
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

            // Obtener direcciones (se pueden extender con joins para traer información de Distrito, Provincia, etc.)
            var direcciones = await _db.DireccionesUsuario
                .Where(d => d.IdUsuario == idUsuario)
                .Select(d => new {
                    d.IdDireccion,
                    d.Calle,
                    d.Numero,
                    d.IdDistrito
                })
                .ToListAsync();

            // Obtener tarjetas
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

        // Endpoint para obtener el perfil público de un usuario
        // Se accede a través de /api/usuarios/perfil-publico/{id}
        [HttpGet("perfil-publico/{id}")]
        public async Task<IActionResult> GetPerfilPublico(int id)
        {
            // Datos básicos del usuario
            var usuario = await _db.Usuarios
                .Where(u => u.IdUsuario == id)
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

            // Subastas activas: productos activos con FechaFin definida y futura
            var subastas = await _db.Productos
                .Where(p => p.IdUsuario == id
                            && p.Activo
                            && p.FechaFin.HasValue
                            && p.FechaFin.Value > DateTime.UtcNow)
                .Select(p => new {
                    p.IdProducto,
                    p.Nombre,
                    p.Descripcion,
                    p.Precio,
                    p.FechaFin,
                    p.RutaImagen,
                    p.NombreImagen
                })
                .ToListAsync();

            // Ventas activas: productos activos sin FechaFin (no en subasta)
            var ventas = await _db.Productos
                .Where(p => p.IdUsuario == id
                            && p.Activo
                            && !p.FechaFin.HasValue)
                .Select(p => new {
                    p.IdProducto,
                    p.Nombre,
                    p.Descripcion,
                    p.Precio,
                    p.FechaRegistro,
                    p.RutaImagen,
                    p.NombreImagen
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
