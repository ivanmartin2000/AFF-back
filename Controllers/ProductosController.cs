using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AFF_back;
using System.ComponentModel.DataAnnotations;

namespace AFF_back.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env; // Para obtener la ruta raíz del proyecto

        public ProductosController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        [HttpPost("publicar")]
        public async Task<IActionResult> PublicarProducto([FromForm] PublicarProductoRequest request)
        {
            // Extraer IdUsuario del token
            if (!int.TryParse(User.FindFirst("IdUsuario")?.Value, out int idUsuario))
                return Unauthorized("No se pudo extraer el usuario.");

            // Validar TipoPublicacion
            var tipo = request.TipoPublicacion?.ToLower() ?? "venta";
            if (tipo != "venta" && tipo != "subasta")
                return BadRequest("TipoPublicacion debe ser 'venta' o 'subasta'.");

            // Para subasta se requiere FechaFinSubasta y OfertaInicial
            if (tipo == "subasta")
            {
                if (!request.FechaFinSubasta.HasValue)
                    return BadRequest("Debe proporcionar FechaFinSubasta para productos en subasta.");
                if (!request.OfertaInicial.HasValue)
                    return BadRequest("Debe proporcionar una OfertaInicial para productos en subasta.");
            }

            // Si el campo Nombre o Descripcion viene vacío, se asigna un valor por defecto.
            var nombre = string.IsNullOrWhiteSpace(request.Nombre) ? "Sin Nombre" : request.Nombre;
            var descripcion = string.IsNullOrWhiteSpace(request.Descripcion) ? "Sin Descripción" : request.Descripcion;

            // Para subasta, se usa OfertaInicial como precio; para venta, el precio ingresado.
            decimal precioFinal = tipo == "subasta" ? request.OfertaInicial.Value : request.Precio;

            // Procesar la imagen (si se envía)
            string rutaImagen = "/public/";
            string nombreImagen = "default.png"; // Valor por defecto
            if (request.Imagen != null && request.Imagen.Length > 0)
            {
                // Define la ruta completa en el sistema de archivos
                string uploadsFolder = @"C:\Users\Ivan\Desktop\ProyectoAFF\AllForFans\public";
                // Asegúrate de que el directorio exista
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                // Usa el nombre original del archivo; podrías agregar lógica para evitar colisiones
                nombreImagen = Path.GetFileName(request.Imagen.FileName);
                string filePath = Path.Combine(uploadsFolder, nombreImagen);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await request.Imagen.CopyToAsync(fileStream);
                }
            }

            // Crear la entidad producto
            var producto = new Producto
            {
                Nombre = nombre,
                Descripcion = descripcion,
                Precio = precioFinal,
                Stock = request.Stock,
                RutaImagen = rutaImagen,
                NombreImagen = nombreImagen,
                Activo = true,
                FechaRegistro = DateTime.UtcNow,
                IdUsuario = idUsuario,
                FechaFin = tipo == "subasta" ? request.FechaFinSubasta : null,
                IdMarca = request.IdMarca,
                IdCategoria = request.IdCategoria,
                TipoPublicacion = tipo // Guarda "venta" o "subasta"
            };

            _db.Productos.Add(producto);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(PublicarProducto), new { id = producto.IdProducto }, producto);
        }

        // Modelo para recibir la solicitud de publicación
        public class PublicarProductoRequest
        {
            [Required]
            public string Nombre { get; set; } = string.Empty;

            [Required]
            public string Descripcion { get; set; } = string.Empty;

            [Required]
            public decimal Precio { get; set; }

            [Required]
            public int Stock { get; set; }

            // Se usará para recibir el archivo de imagen
            public IFormFile? Imagen { get; set; }

            [Required]
            public string TipoPublicacion { get; set; } = "venta"; // "venta" o "subasta"

            public DateTime? FechaFinSubasta { get; set; }

            public decimal? OfertaInicial { get; set; }

            [Required]
            public int IdMarca { get; set; }

            [Required]
            public int IdCategoria { get; set; }
        }
    }
}
