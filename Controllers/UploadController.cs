using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Threading.Tasks;

namespace AFF_back.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public UploadController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost("imagen")]
        public async Task<IActionResult> SubirImagen([FromForm] IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
                return BadRequest("No se seleccionó ningún archivo.");

            // Define la carpeta destino. Se recomienda usar el ContentRootPath o WebRootPath.
            var uploadsFolder = Path.Combine(_env.ContentRootPath, "public");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Crea un nombre único para el archivo
            var fileName = "imagen_" + DateTime.UtcNow.Ticks + Path.GetExtension(archivo.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Guarda el archivo en disco
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            // Retorna la ruta relativa que se almacenará en la base de datos (útil para luego mostrar la imagen)
            var rutaRelativa = "/public/" + fileName;
            return Ok(new { rutaImagen = rutaRelativa });
        }
    }
}
