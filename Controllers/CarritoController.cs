using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AFF_back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CarritoController : Controller
    {
        private readonly AppDbContext _db;
        public CarritoController(AppDbContext db)
        {
            _db = db;
        }

        [HttpPost("productosComprados")]
        public async Task<IActionResult> ProductosComprados([FromBody] ProductosCompradosRequest request)
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
            return CreatedAtAction(nameof(ProductosComprados), new { id = nuevoUsuario.IdUsuario }, nuevoUsuario);
        }
     
        // GET: CarritoController
        public ActionResult Index()
        {
            return View();
        }

        // GET: CarritoController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: CarritoController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: CarritoController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: CarritoController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: CarritoController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: CarritoController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: CarritoController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }

    public class ProductosCompradosRequest
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
