using System;

namespace AFF_back
{
    public class Tarjeta
    {
        public int IdTarjeta { get; set; }
        public int IdUsuario { get; set; }
        public string NumeroTarjeta { get; set; } = null!;
        public string Titular { get; set; } = null!;
        public DateTime FechaExpiracion { get; set; }
        public string? CVV { get; set; }
        public string? TipoTarjeta { get; set; }
        public DateTime? FechaRegistro { get; set; }

        // Propiedad de navegación
        public Usuario Usuario { get; set; } = null!;
    }
}
