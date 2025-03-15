using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AFF_back;

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

        [HttpPost("registro")]
        public async Task<IActionResult> Registro([FromBody] RegistroRequest request)
        {
            // Verificar si ya existe un usuario con el mismo correo
            var usuarioExistente = await _db.Usuarios.FirstOrDefaultAsync(u => u.Correo == request.Correo);
            if (usuarioExistente != null)
            {
                return BadRequest("El correo ya está registrado.");
            }

            // Crear nuevo usuario
            var nuevoUsuario = new Usuario
            {
                Nombres = request.Nombre,
                Apellidos = request.Apellido,
                Correo = request.Correo,
                Clave = request.Clave, // Idealmente deberías aplicar hash a la contraseña
                Activo = true,
                Resetear = false,
                FechaRegistro = DateTime.UtcNow,
                // Si es creador de contenido, asignamos Nivel 2; de lo contrario, Nivel 3 (por ejemplo)
                Nivel = request.EsCreador ? 2 : 3,
                Descripcion = request.Descripcion, // Opcional, puede venir del formulario
                ImagenPerfil = null // Se cargará en otro momento
            };

            // Aquí podrías guardar también datos extra de creador (p.ej., SocialAccount) en otra tabla o en campos adicionales
            // según la lógica de tu aplicación.

            _db.Usuarios.Add(nuevoUsuario);
            await _db.SaveChangesAsync();

            // Retornamos CreatedAtAction (puedes modificar la respuesta según convenga)
            return CreatedAtAction(nameof(Registro), new { id = nuevoUsuario.IdUsuario }, nuevoUsuario);
        }

        // Nuevo endpoint para obtener el perfil del usuario logueado
        [Authorize]
        [HttpGet("perfil")]
        public async Task<IActionResult> GetPerfilUsuario()
        {
            // Extraer el IdUsuario del token
            if (!int.TryParse(User.FindFirst("IdUsuario")?.Value, out int idUsuario))
            {
                return Unauthorized("No se pudo extraer el usuario.");
            }

            // Buscar el usuario en la base de datos y proyectar los datos deseados
            var usuario = await _db.Usuarios
                .Where(u => u.IdUsuario == idUsuario)
                .Select(u => new
                {
                    u.IdUsuario,
                    u.Nombres,
                    u.Apellidos,
                    u.Correo,
                    u.ImagenPerfil,
                    u.Descripcion,
                    u.Nivel,
                    u.FechaRegistro
                })
                .FirstOrDefaultAsync();

            if (usuario == null)
            {
                return NotFound("Perfil no encontrado.");
            }

            return Ok(usuario);
        }
    }

    public class RegistroRequest
    {
        public string Nombre { get; set; } = null!;
        public string Apellido { get; set; } = null!;
        public string Correo { get; set; } = null!;
        public string Clave { get; set; } = null!;
        public bool EsCreador { get; set; }
        // Campo para la cuenta verificada de una red social (Instagram, X, Twitch, YouTube, etc.)
        public string? SocialAccount { get; set; }
        // Opcional, si deseas almacenar alguna descripción adicional (por ejemplo, información del creador)
        public string? Descripcion { get; set; }
    }
}
