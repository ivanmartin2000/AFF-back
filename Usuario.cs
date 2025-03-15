using System;
using System.Collections.Generic;

namespace AFF_back
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string Nombres { get; set; } = null!;
        public string Apellidos { get; set; } = null!;
        public string Correo { get; set; } = null!;
        public string Clave { get; set; } = null!;
        public bool Activo { get; set; }
        public bool Resetear { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int Nivel { get; set; }
        public string? Descripcion { get; set; }
        public string? ImagenPerfil { get; set; }

        // Relaciones existentes
        public ICollection<UsuarioFavoritos> Favoritos { get; set; } = new List<UsuarioFavoritos>();
        public ICollection<UsuarioFavoritos> FavoritosDe { get; set; } = new List<UsuarioFavoritos>();
        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
        public Billetera? Billetera { get; set; }

        // Nuevas relaciones
        public ICollection<Tarjeta> Tarjetas { get; set; } = new List<Tarjeta>();
        public ICollection<DireccionUsuario> Direcciones { get; set; } = new List<DireccionUsuario>();
    }
}
