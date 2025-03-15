using Microsoft.AspNetCore.Mvc;
using System.Linq;
using AFF_back; // Asegúrate de ajustar el namespace según tu estructura

namespace AFF_back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BilleteraController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BilleteraController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Billetera/usuario/{idUsuario}
        [HttpGet("usuario/{idUsuario}")]
        public IActionResult GetBilleteraByUser(int idUsuario)
        {
            // Utilizando LINQ para buscar la billetera del usuario
            var billetera = _context.Billeteras
                                    .Where(b => b.IdUsuario == idUsuario)
                                    .Select(b => new
                                    {
                                        b.IdBilletera,
                                        b.IdUsuario,
                                        b.Monto
                                    })
                                    .FirstOrDefault();

            if (billetera == null)
            {
                return NotFound("No se encontró billetera para el usuario especificado.");
            }

            return Ok(billetera);
        }
    }
}
