using Microsoft.EntityFrameworkCore;

namespace AFF_back
{
    public class Billetera
    {
        public int IdBilletera { get; set; }
        public int IdUsuario { get; set; }
        public decimal Monto { get; set; } = 0;

        // Propiedad de navegación
        public Usuario Usuario { get; set; } = null!;
    }
}
