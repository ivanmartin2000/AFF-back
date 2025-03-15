using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AFF_back;


namespace AFF_back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            // Buscar el usuario por correo y asegurarse de que esté activo
            var usuario = await _db.Usuarios
                .FirstOrDefaultAsync(u => u.Correo == request.Correo && u.Activo);
            if (usuario == null)
                return Unauthorized("Correo o contraseña inválidos.");

            // Validar la contraseña (aquí se compara en texto plano; idealmente se usaría hash)
            if (usuario.Clave != request.Clave)
                return Unauthorized("Correo o contraseña inválidos.");

            // Generar el token JWT
            var token = GenerarToken(usuario);

            return new LoginResponse
            {
                Token = token,
                NombreCompleto = $"{usuario.Nombres} {usuario.Apellidos}",
                Nivel = usuario.Nivel
            };
        }

        private string GenerarToken(Usuario usuario)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim("IdUsuario", usuario.IdUsuario.ToString()),
                new Claim("Nivel", usuario.Nivel.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Correo),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(3),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}