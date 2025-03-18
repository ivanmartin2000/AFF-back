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

            // Verificar que el producto esté en una subasta (TipoPublicacion debe ser "Subasta")
            if (producto.TipoPublicacion != "subasta")
                return BadRequest("El producto no está en una subasta.");

            // Verificar que el usuario no puje en su propio producto
            if (producto.IdUsuario == idUsuario)
                return BadRequest("No puedes ofertar en tu propio producto.");

            // Verificar que la subasta esté activa (por ejemplo, que FechaFin exista y sea futura)
            if (!producto.FechaFin.HasValue || producto.FechaFin.Value < DateTime.UtcNow)
                return BadRequest("La subasta ha finalizado.");

            // Verificar que la puja es mayor que el monto actual de la puja (si existe)
            var pujaActual = await _db.Pujas
                .Where(p => p.IdProducto == request.IdProducto)
                .OrderByDescending(p => p.Monto)
                .FirstOrDefaultAsync();

            if (pujaActual != null && request.Monto <= pujaActual.Monto)
                return BadRequest("La puja debe ser mayor que la puja actual.");

            // Crear la puja
            var puja = new Puja
            {
                IdProducto = request.IdProducto,
                IdUsuario = idUsuario,
                Monto = request.Monto,
                FechaPuja = DateTime.UtcNow,
                IdUsuarioComprador = idUsuario
            };

            try
            {
                // Insertamos la nueva puja en la base de datos
                _db.Pujas.Add(puja);

                // Actualizamos el precio del producto con el monto de la nueva puja
                producto.Precio = request.Monto; // Actualizamos el precio con la nueva puja

                // Verificamos si la subasta ha llegado a su fin
                if (producto.FechaFin.HasValue && producto.FechaFin.Value <= DateTime.UtcNow)
                {
                    // Actualizamos el producto para marcar la subasta como finalizada
                    producto.TipoPublicacion = "Vendido";  // O cualquier otro tipo de publicación
                    producto.Activo = false;  // Hacemos el producto inactivo
                }

                // Guardamos los cambios tanto de la puja como del producto
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Capturamos la excepción para ver el detalle del error
                Console.WriteLine($"Error al guardar la puja: {ex.Message}");
                return StatusCode(500, $"Error al guardar la puja: {ex.InnerException?.Message}");
            }

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
