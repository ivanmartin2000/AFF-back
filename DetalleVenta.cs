using System;

namespace AFF_back
{
    public class DetalleVenta
    {
        public int IdDetalleVenta { get; set; }  // Clave primaria
        public int IdVenta { get; set; }
        public int IdProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal Total { get; set; }

        // Propiedades de navegación (opcional)
        public Venta Venta { get; set; } = null!;
        public Producto Producto { get; set; } = null!;
    }
}
