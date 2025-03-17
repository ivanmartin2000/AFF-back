using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using AFF_back;

[ApiController]
[Route("api/[controller]")]
public class MarcasController : ControllerBase
{
    private readonly AppDbContext _db;
    public MarcasController(AppDbContext db)
    {
        _db = db;
    }

    // POST /api/marcas
    [HttpPost]
    public async Task<IActionResult> CrearMarca([FromBody] CrearMarcaRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Descripcion))
            return BadRequest("La descripción de la marca no puede estar vacía.");

        var nuevaMarca = new AFF_back.Marca
        {
            Descripcion = request.Descripcion,
            Activo = true,
            FechaRegistro = DateTime.UtcNow
        };

        _db.Marcas.Add(nuevaMarca);
        await _db.SaveChangesAsync();

        return Ok(new { idMarca = nuevaMarca.IdMarca, descripcion = nuevaMarca.Descripcion });
    }
}

public class CrearMarcaRequest
{
    public string Descripcion { get; set; } = string.Empty;
}
