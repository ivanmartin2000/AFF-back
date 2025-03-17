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
    public class PujaController : ControllerBase
    {
        private readonly AppDbContext _db;
        public PujaController(AppDbContext db)
        {
            _db = db;
        }

        [HttpPost("realizar")]
        public async Task<IActionResult> RealizarPuja([FromBody] PujarRequest request)
        {
            // Extraer el IdUsuario del token (quien hace la puja)
            if (!int.TryParse(User.FindFirst("IdUsuario")?.Value, out int idUsuario))
                return Unauthorized("No se pudo extraer el usuario.");

            // Obtener el producto para el que se realiza la puja
            var producto = await _db.Productos.FirstOrDefaultAsync(p => p.IdProducto == request.IdProducto && p.Activo);
            if (producto == null)
                return BadRequest("Producto no encontrado o inactivo.");

            // Verificar que el usuario no puje en su propio producto
            if (producto.IdUsuario == idUsuario)
                return BadRequest("No puedes ofertar en tu propio producto.");

            // Verificar que la subasta esté activa (por ejemplo, que FechaFin exista y sea futura)
            if (!producto.FechaFin.HasValue || producto.FechaFin.Value < DateTime.UtcNow)
                return BadRequest("La subasta ha finalizado.");

            // Crear la puja (asumiendo que tienes la entidad Puja)
            var puja = new Puja
            {
                IdSubasta = request.IdSubasta,  // Asumiendo que tu producto subastado se relaciona con una subasta
                IdUsuario = idUsuario,
                Monto = request.Monto,
                FechaPuja = DateTime.UtcNow
            };

            _db.Pujas.Add(puja);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Puja realizada con éxito" });
        }
    }

    // Modelo de solicitud para la puja
    public class PujarRequest
    {
        public int IdProducto { get; set; }
        public int IdSubasta { get; set; }  // Si usas una tabla de subastas; si no, puedes omitirlo
        public decimal Monto { get; set; }
    }
}
