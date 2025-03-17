using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AFF_back;

namespace AFF_back.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CarritoController : ControllerBase
    {
        private readonly AppDbContext _db;
        public CarritoController(AppDbContext db)
        {
            _db = db;
        }

        // GET: api/carrito/usuario/{idUsuario}
        [HttpGet("usuario/{idUsuario}")]
        public async Task<IActionResult> GetCarritoByUser(int idUsuario)
        {
            var items = await _db.Carritos
                .Where(c => c.IdCliente == idUsuario)
                .Select(c => new
                {
                    c.IdCarrito,
                    c.IdProducto,
                    c.Cantidad
                })
                .ToListAsync();

            return Ok(items);
        }

        // POST: api/carrito/agregar
        [HttpPost("agregar")]
        public async Task<IActionResult> AgregarAlCarrito([FromBody] CarritoRequest request)
        {
            if (!int.TryParse(User.FindFirst("IdUsuario")?.Value, out int idUsuario))
                return Unauthorized("No se pudo extraer el usuario.");

            var existingItem = await _db.Carritos.FirstOrDefaultAsync(c => c.IdCliente == idUsuario && c.IdProducto == request.IdProducto);
            if (existingItem != null)
            {
                existingItem.Cantidad += request.Cantidad;
            }
            else
            {
                var nuevoItem = new Carrito
                {
                    IdCliente = idUsuario,
                    IdProducto = request.IdProducto,
                    Cantidad = request.Cantidad
                };
                _db.Carritos.Add(nuevoItem);
            }
            await _db.SaveChangesAsync();
            return Ok(new { message = "Producto agregado al carrito." });
        }
    }

    public class CarritoRequest
    {
        public int IdProducto { get; set; }
        public int Cantidad { get; set; }
    }
}
