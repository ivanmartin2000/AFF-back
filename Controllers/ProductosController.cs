using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AFF_back;

namespace AFF_back.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosController : ControllerBase
    {
        private readonly AppDbContext _db;
        public ProductosController(AppDbContext db)
        {
            _db = db;
        }

        [HttpPost("publicar")]
        public async Task<IActionResult> PublicarProducto([FromBody] PublicarProductoRequest request)
        {
            // Extraer IdUsuario
            if (!int.TryParse(User.FindFirst("IdUsuario")?.Value, out int idUsuario))
                return Unauthorized("No se pudo extraer el usuario.");

            // Si es subasta, forzar precio = 0 (o bloquearlo)
            decimal precioFinal = request.TipoPublicacion == "subasta" ? 0 : request.Precio;

            // Crear la entidad producto
            var producto = new Producto
            {
                Nombre = request.Nombre,
                Descripcion = request.Descripcion,
                Precio = precioFinal,
                Stock = request.Stock,
                RutaImagen = request.RutaImagen,
                NombreImagen = request.NombreImagen,
                Activo = true,
                FechaRegistro = DateTime.UtcNow,
                IdUsuario = idUsuario,
                // Si es subasta, se asigna FechaFin, de lo contrario null
                FechaFin = request.TipoPublicacion == "subasta" ? request.FechaFinSubasta : null,
                // Asignar marca
                IdMarca = request.IdMarca,
                // Opcional: Asignar categoría si lo necesitas
                IdCategoria = request.IdCategoria
            };

            _db.Productos.Add(producto);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(PublicarProducto), new { id = producto.IdProducto }, producto);
        }
    }


    // Modelo para recibir la solicitud
    public class PublicarProductoRequest
    {
        public string Nombre { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string? RutaImagen { get; set; }
        public string? NombreImagen { get; set; }
        public string TipoPublicacion { get; set; } = "venta"; // "venta" o "subasta"
        public DateTime? FechaFinSubasta { get; set; }
        public decimal? OfertaInicial { get; set; }
        public int IdMarca { get; set; } // Nuevo campo para la marca
        public int IdCategoria { get; set; } // Opcional si necesitas categoría
    }

}
