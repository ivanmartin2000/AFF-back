using AFF_back;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Agrega tus DbSet
    public DbSet<Venta> Ventas { get; set; } = null!;
    public DbSet<DetalleVenta> DetalleVentas { get; set; } = null!;
    public DbSet<Usuario> Usuarios { get; set; } = null!;
    public DbSet<UsuarioFavoritos> UsuarioFavoritos { get; set; } = null!;
    public DbSet<Billetera> Billeteras { get; set; } = null!;
    public DbSet<Producto> Productos { get; set; } = null!;
    public DbSet<Categoria> Categorias { get; set; } = null!;
    public DbSet<Tarjeta> Tarjetas { get; set; } = null!;
    public DbSet<DireccionUsuario> DireccionesUsuario { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ----------------- USUARIO -----------------
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("USUARIO");
            entity.HasKey(e => e.IdUsuario);
        });

        // ----------------- USUARIO_FAVORITOS -----------------
        modelBuilder.Entity<UsuarioFavoritos>(entity =>
        {
            entity.ToTable("USUARIO_FAVORITOS");
            entity.HasKey(e => e.IdFavorito);
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("GETDATE()");

            entity.HasOne(uf => uf.Usuario)
                  .WithMany(u => u.Favoritos)
                  .HasForeignKey(uf => uf.IdUsuario)
                  .IsRequired()
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(uf => uf.Vendedor)
                  .WithMany(u => u.FavoritosDe)
                  .HasForeignKey(uf => uf.IdFavoritoUsuario)
                  .IsRequired()
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ----------------- BILLETERA -----------------
        modelBuilder.Entity<Billetera>(entity =>
        {
            entity.ToTable("BILLETERA");
            entity.HasKey(e => e.IdBilletera);
            entity.Property(e => e.Monto)
                  .HasPrecision(18, 2)
                  .HasDefaultValue(0);

            entity.HasOne(e => e.Usuario)
                  .WithOne(u => u.Billetera)
                  .HasForeignKey<Billetera>(e => e.IdUsuario)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ----------------- PRODUCTO -----------------
        modelBuilder.Entity<Producto>(entity =>
        {
            entity.ToTable("PRODUCTO");
            entity.HasKey(e => e.IdProducto);
            entity.Property(e => e.Precio).HasPrecision(18, 2);

            entity.HasOne(e => e.Usuario)
                  .WithMany(u => u.Productos)
                  .HasForeignKey(e => e.IdUsuario)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ----------------- CATEGORIA -----------------
        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.ToTable("CATEGORIA");
            entity.HasKey(e => e.IdCategoria);
            entity.Property(e => e.Descripcion)
                  .IsRequired()
                  .HasMaxLength(200);
            entity.Property(e => e.Activo).IsRequired();
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
        });

        // ----------------- TARJETA -----------------
        modelBuilder.Entity<Tarjeta>(entity =>
        {
            entity.ToTable("TARJETA");
            entity.HasKey(e => e.IdTarjeta);
            entity.Property(e => e.NumeroTarjeta).IsRequired();
            entity.Property(e => e.Titular).IsRequired();
            entity.Property(e => e.FechaExpiracion).IsRequired();

            entity.HasOne(e => e.Usuario)
                  .WithMany(u => u.Tarjetas)
                  .HasForeignKey(e => e.IdUsuario)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ----------------- DIRECCION_USUARIO -----------------
        modelBuilder.Entity<DireccionUsuario>(entity =>
        {
            entity.ToTable("DIRECCION_USUARIO");
            entity.HasKey(e => e.IdDireccion);
            entity.Property(e => e.Calle).IsRequired();
            entity.Property(e => e.Numero).IsRequired();
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("GETDATE()");

            entity.HasOne(e => e.Usuario)
                  .WithMany(u => u.Direcciones)
                  .HasForeignKey(e => e.IdUsuario)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ----------------- VENTA -----------------
        modelBuilder.Entity<Venta>(entity =>
        {
            entity.ToTable("VENTA");
            entity.HasKey(e => e.IdVenta); // Indica que IdVenta es la PK

            // Ejemplo de mapeo de columnas adicionales, si lo requieres:
            // entity.Property(e => e.MontoTotal).HasPrecision(18, 2);

            // Si IdCliente referencia un usuario (en lugar de la tabla CLIENTE):
            entity.HasOne(e => e.Usuario)
                  .WithMany() // o con una colección en Usuario si deseas
                  .HasForeignKey(e => e.IdCliente)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ----------------- DETALLE_VENTA -----------------
        modelBuilder.Entity<DetalleVenta>(entity =>
        {
            entity.ToTable("DETALLE_VENTA");
            entity.HasKey(e => e.IdDetalleVenta);
            entity.Property(e => e.Total).HasPrecision(18, 2);

            entity.HasOne(e => e.Venta)
                  .WithMany(v => v.DetalleVentas)
                  .HasForeignKey(e => e.IdVenta)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Producto)
                  .WithMany() // o define una colección en Producto
                  .HasForeignKey(e => e.IdProducto)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
