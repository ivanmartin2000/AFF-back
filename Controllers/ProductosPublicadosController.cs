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
    public class ProductosPublicadosController : ControllerBase
    {
        private readonly AppDbContext _db;
        public ProductosPublicadosController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetProductosPublicados()
        {
            // Extraer el IdUsuario del token (se asume que la claim se llama "IdUsuario")
            if (!int.TryParse(User.FindFirst("IdUsuario")?.Value, out int idUsuario))
                return Unauthorized("No se pudo extraer el usuario.");

            // Consulta: seleccionar productos publicados a la venta (Activo y sin FechaFin) del usuario
            var productosPublicados = await _db.Productos
                .Where(p => p.Activo && p.FechaFin == null && p.IdUsuario == idUsuario)
                .Select(p => new
                {
                    idProducto = p.IdProducto,
                    nombre = p.Nombre,
                    descripcion = p.Descripcion,
                    precio = p.Precio,
                    fechaRegistro = p.FechaRegistro,
                    rutaImagen = p.RutaImagen,
                    nombreImagen = p.NombreImagen
                })
                .ToListAsync();

            return Ok(productosPublicados);
        }
    }
}
