using System;

namespace AFF_back
{
    public class Puja
    {
        // Clave primaria
        public int IdPuja { get; set; }

        // Campo opcional para relacionar con una subasta o producto en subasta
        public int? IdSubasta { get; set; }

        // En algunos esquemas, este campo puede ser usado para otros fines (si es necesario)
        public int? IdUsuario { get; set; }

        // Monto ofertado
        public decimal Monto { get; set; }

        // Fecha en que se realizó la puja
        public DateTime? FechaPuja { get; set; }

        // Identificador del usuario que realizó la puja
        public int? IdUsuarioComprador { get; set; }

        // Propiedad de navegación opcional, si deseas relacionarla con USUARIO (el comprador)
        public Usuario? UsuarioComprador { get; set; }
    }
}
