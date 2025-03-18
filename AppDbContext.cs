using AFF_back;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // DbSet existentes
    public DbSet<Venta> Ventas { get; set; } = null!;
    public DbSet<DetalleVenta> DetalleVentas { get; set; } = null!;
    public DbSet<Usuario> Usuarios { get; set; } = null!;
    public DbSet<UsuarioFavoritos> UsuarioFavoritos { get; set; } = null!;
    public DbSet<Billetera> Billeteras { get; set; } = null!;
    public DbSet<Producto> Productos { get; set; } = null!;
    public DbSet<Categoria> Categorias { get; set; } = null!;
    public DbSet<Tarjeta> Tarjetas { get; set; } = null!;
    public DbSet<DireccionUsuario> DireccionesUsuario { get; set; } = null!;
    public DbSet<Puja> Pujas { get; set; } = null!;

    // Nuevo DbSet para envíos
    public DbSet<Envio> Envios { get; set; } = null!;
    public DbSet<Carrito> Carritos { get; set; } = null!;
    public DbSet<Marca> Marcas { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ----------------- CARRITO -----------------
        modelBuilder.Entity<Carrito>(entity =>
        {
            entity.ToTable("CARRITO");
            entity.HasKey(e => e.IdCarrito);
            // Aquí podrías agregar propiedades adicionales o configuraciones de columna si lo requieres.
        });

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
            entity.Property(e => e.FechaRegistro)
                  .HasColumnType("datetime");
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
            entity.HasKey(e => e.IdVenta);
            // Si IdCliente referencia USUARIO:
            entity.HasOne(e => e.Usuario)
                  .WithMany()
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
                  .WithMany() // Puedes definir una colección en Producto si lo necesitas
                  .HasForeignKey(e => e.IdProducto)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ----------------- PUJA -----------------
        modelBuilder.Entity<Puja>(entity =>
        {
            entity.ToTable("PUJA");
            entity.HasKey(e => e.IdPuja);

            // Configuración de las propiedades
            entity.Property(e => e.Monto).HasPrecision(18, 2);
            entity.Property(e => e.FechaPuja).HasColumnType("datetime");

            // Relación con el producto (IdProducto)
            entity.HasOne(e => e.Producto)  // Relación con la entidad Producto
                .WithMany()  // El producto puede tener muchas pujas
                .HasForeignKey(e => e.IdProducto)  // Usamos la propiedad IdProducto para la clave foránea
                .OnDelete(DeleteBehavior.Restrict);  // No eliminar las pujas si el producto se elimina

            // Relación con el usuario que realiza la puja (IdUsuario)
            entity.HasOne(e => e.Usuario)  // Relación con la entidad Usuario
                .WithMany()  // Un usuario puede hacer muchas pujas
                .HasForeignKey(e => e.IdUsuario)  // Usamos la propiedad IdUsuario para la clave foránea
                .OnDelete(DeleteBehavior.Cascade);  // Si un usuario es eliminado, eliminar todas sus pujas

            // Relación con el usuario comprador (IdUsuarioComprador)
            entity.HasOne(e => e.UsuarioComprador)  // Relación con la entidad Usuario (quien compra, si aplica)
                .WithMany()  // Un comprador puede tener muchas pujas
                .HasForeignKey(e => e.IdUsuarioComprador)  // Usamos la propiedad IdUsuarioComprador para la clave foránea
                .OnDelete(DeleteBehavior.Restrict);  // Evitar eliminar las pujas si el usuario comprador es eliminado
        });
        // ----------------- MARCA -----------------
        modelBuilder.Entity<Marca>(entity =>
        {
            entity.ToTable("MARCA");
            entity.HasKey(e => e.IdMarca);
            entity.Property(e => e.Descripcion)
                  .IsRequired()
                  .HasMaxLength(200);
            entity.Property(e => e.Activo).IsRequired();
            entity.Property(e => e.FechaRegistro)
                  .HasColumnType("datetime");
        });
        // ----------------- ENVIO -----------------
        modelBuilder.Entity<Envio>(entity =>
        {
            entity.ToTable("ENVIO");
            entity.HasKey(e => e.IdEnvio);
            entity.Property(e => e.EstadoEnvio).IsRequired();
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("GETDATE()");

            entity.HasOne(e => e.Venta)
                  .WithOne() // Se asume que cada venta tiene un único envío
                  .HasForeignKey<Envio>(e => e.IdVenta)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
