using System;

namespace AFF_back
{
    public class Puja
    {
        // Clave primaria
        public int IdPuja { get; set; }

        // Relación con la subasta (o producto en subasta)
        public int? IdProducto { get; set; }  // Cambiado de IdSubasta a IdProducto si la puja se realiza sobre un producto

        // Monto ofertado
        public decimal Monto { get; set; }

        // Fecha en que se realizó la puja
        public DateTime FechaPuja { get; set; }  // FechaPuja ahora no es nullable

        // Identificador del usuario que realizó la puja
        public int IdUsuario { get; set; }  // Este campo no es nullable, ya que el usuario que realiza la puja es obligatorio

        // Identificador del usuario que compró el producto (opcional)
        public int? IdUsuarioComprador { get; set; }

        // Propiedad de navegación opcional para relacionar con el producto subastado
        public Producto Producto { get; set; }  // Relación con Producto

        // Propiedad de navegación para el usuario que realizó la puja
        public Usuario Usuario { get; set; }  // Relación con Usuario (quien realiza la puja)

        // Propiedad de navegación opcional para el usuario comprador
        public Usuario UsuarioComprador { get; set; }  // Relación con Usuario (quien compra el producto)
    }
}
