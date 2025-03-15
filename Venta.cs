using AFF_back;

public class Venta
{
    public int IdVenta { get; set; } // Clave primaria

    // Otras columnas:
    public int IdCliente { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public DateTime? FechaVenta { get; set; }
    public decimal TotalProducto { get; set; }
    public decimal MontoTotal { get; set; }
    public string? Contacto { get; set; }
    public int? IdDistrito { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? IdTransaccion { get; set; }

    // Relación con DetalleVenta
    public ICollection<DetalleVenta> DetalleVentas { get; set; } = new List<DetalleVenta>();
}
