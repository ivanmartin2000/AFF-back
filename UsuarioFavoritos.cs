namespace AFF_back
{
    public class UsuarioFavoritos
    {
        public int IdFavorito { get; set; }
        public int IdUsuario { get; set; }          // Usuario que guarda el favorito
        public int IdFavoritoUsuario { get; set; }    // Usuario que es marcado como favorito
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        // Propiedades de navegación
        public Usuario Usuario { get; set; } = null!;
        public Usuario Vendedor { get; set; } = null!;
    }

}
