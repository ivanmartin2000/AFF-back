namespace AFF_back
{
    public class Carrito
    {
        public int IdCarrito { get; set; }
        public int IdCliente { get; set; }  // Usamos el IdUsuario del comprador
        public int IdProducto { get; set; }
        public int Cantidad { get; set; }
    }
}
