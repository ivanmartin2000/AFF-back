namespace AFF_back
{
    public class Producto
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public int IdMarca { get; set; }
        public int IdCategoria { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string RutaImagen { get; set; } = null!;
        public string NombreImagen { get; set; } = null!;
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaFin { get; set; }  // Para subasta

        // Nueva columna para identificar al vendedor (quien publicó el producto)
        public int IdUsuario { get; set; }

        // Propiedad de navegación
        public Usuario Usuario { get; set; } = null!;
    }
}
