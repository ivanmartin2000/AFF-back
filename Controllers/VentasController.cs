using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class VentasController : ControllerBase
{
    private readonly AppDbContext _db;
    public VentasController(AppDbContext db)
    {
        _db = db;
    }
    [Authorize]
    [HttpGet("productos-vendidos")]
    public async Task<IActionResult> GetProductosVendidos()
    {
        // Extrae el IdUsuario del token (que representa al vendedor)
        if (!int.TryParse(User.FindFirst("IdUsuario")?.Value, out int idUsuario))
            return Unauthorized("No se pudo extraer el usuario.");

        // Consulta: desde DetalleVentas, unir con Venta y Producto,
        // y filtrar los productos cuyo vendedor (por ejemplo, en Producto.IdUsuario) sea el usuario logueado.
        var productosVendidos = await (
            from detalle in _db.DetalleVentas
            join venta in _db.Ventas on detalle.IdVenta equals venta.IdVenta
            join producto in _db.Productos on detalle.IdProducto equals producto.IdProducto
            where producto.IdUsuario == idUsuario
            select new
            {
                id = producto.IdProducto,
                nombre = producto.Nombre,
                fechaVenta = venta.FechaVenta,
                precio = detalle.Total
            }
        ).ToListAsync();

        return Ok(productosVendidos);
    }

}
