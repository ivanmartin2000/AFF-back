using System;

namespace AFF_back
{
    public class Envio
    {
        public int IdEnvio { get; set; }
        public int IdVenta { get; set; }
        public string EstadoEnvio { get; set; } = "Pendiente"; // Ejemplo: "Pendiente", "En Tránsito", "Recibido", "Cancelado"
        public DateTime? FechaEnvio { get; set; }
        public string? TrackingNumber { get; set; }
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        // Propiedad de navegación a la venta
        public Venta Venta { get; set; } = null!;
    }
}
