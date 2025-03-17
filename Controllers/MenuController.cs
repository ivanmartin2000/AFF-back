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
    public class MenuController : ControllerBase
    {
        private readonly AppDbContext _db;
        public MenuController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("principal")]
        public async Task<IActionResult> GetMenuPrincipal()
        {
            // Extraemos la claim "IdUsuario"
            var idClaim = User.FindFirst("IdUsuario")?.Value;
            System.Diagnostics.Debug.WriteLine("Claim IdUsuario: " + idClaim);

            if (!int.TryParse(idClaim, out int idUsuario))
            {
                return Unauthorized("No se pudo extraer el usuario.");
            }

            // Obtener los favoritos del usuario logueado
            var favoritos = await _db.UsuarioFavoritos
                .Where(fav => fav.IdUsuario == idUsuario)
                .Include(fav => fav.Vendedor)
                .Select(fav => new
                {
                    // Usamos la propiedad "idUsuario" para identificar al vendedor
                    idUsuario = fav.Vendedor.IdUsuario,
                    nombre = fav.Vendedor.Nombres + " " + fav.Vendedor.Apellidos,
                    imagenPerfil = fav.Vendedor.ImagenPerfil ?? string.Empty,
                    descripcion = fav.Vendedor.Descripcion ?? string.Empty,
                    // Último producto en venta
                    productoVenta = _db.Productos
                        .Where(p => p.IdUsuario == fav.Vendedor.IdUsuario && p.Activo)
                        .OrderByDescending(p => p.FechaRegistro)
                        .Select(p => new
                        {
                            p.Nombre,
                            p.Descripcion,
                            p.Precio,
                            p.RutaImagen,
                            p.NombreImagen
                        }).FirstOrDefault(),
                    // Último producto en subasta, manejando FechaFin de forma segura
                    productoSubasta = _db.Productos
                        .Where(p => p.IdUsuario == fav.Vendedor.IdUsuario && p.Activo)
                        .OrderByDescending(p => p.FechaRegistro)
                        .Select(p => new
                        {
                            p.Nombre,
                            p.Descripcion,
                            p.Precio,
                            p.RutaImagen,
                            p.NombreImagen,
                            FechaFin = p.FechaFin.HasValue ? p.FechaFin.Value : (DateTime?)null
                        }).FirstOrDefault()
                })
                .ToListAsync();

            // Extraer los idUsuario de los vendedores favoritos para excluirlos de sugerencias
            var favoritosIds = favoritos.Select(f => f.idUsuario).ToList();
            // Obtener sugerencias: vendedores de Nivel=2 (vendedores) que no sean el usuario logueado ni ya estén en favoritos
            var sugerencias = await _db.Usuarios
                .Where(u => u.IdUsuario != idUsuario && u.Nivel == 2 && !favoritosIds.Contains(u.IdUsuario))
                .OrderBy(r => Guid.NewGuid())
                .Take(3)
                .Select(u => new {
                    idUsuario = u.IdUsuario,
                    nombre = u.Nombres + " " + u.Apellidos,
                    imagenPerfil = u.ImagenPerfil ?? string.Empty
                })
                .ToListAsync();

            var result = new
            {
                favoritos = favoritos,
                sugerencias = sugerencias
            };

            return Ok(result);
        }

        [HttpGet("categorias")]
        public async Task<IActionResult> GetCategorias()
        {
            var categorias = await _db.Categorias
                .Take(200)
                .Select(c => new
                {
                    id = c.IdCategoria,
                    descripcion = c.Descripcion,
                    activo = c.Activo,
                    fechaRegistro = c.FechaRegistro
                })
                .ToListAsync();

            return Ok(categorias);
        }
    }
}
