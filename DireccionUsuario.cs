using System;

namespace AFF_back
{
    public class DireccionUsuario
    {
        public int IdDireccion { get; set; }
        public int IdUsuario { get; set; }
        public string Calle { get; set; } = null!;
        public int Numero { get; set; }
        public int IdDistrito { get; set; }
        public DateTime FechaRegistro { get; set; }

        // Propiedad de navegación
        public Usuario Usuario { get; set; } = null!;

        // Si tienes entidades para Distrito, Provincia y Departamento, podrías agregarlas aquí
        // public Distrito Distrito { get; set; } = null!;
    }
}
