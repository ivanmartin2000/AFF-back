namespace AFF_back
{
    public class LoginResponse
    {
        public string Token { get; set; } = null!;
        public string NombreCompleto { get; set; } = null!;
        public int Nivel { get; set; }
    }
}
