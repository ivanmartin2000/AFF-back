
using System.ComponentModel.DataAnnotations;

namespace AFF_back
{
    public class Marca
    {
        [Key]
        public int IdMarca { get; set; }
        public string Descripcion { get; set; } = null!;
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
        // ... más propiedades si necesitas
    }
}
