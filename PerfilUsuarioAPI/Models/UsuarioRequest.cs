namespace PerfilUsuarioAPI.Models;

public class UsuarioRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool Suspendido { get; set; }
    public List<int> RolIds { get; set; } = new();
}
