namespace PerfilUsuarioAPI.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool Suspendido { get; set; }
    public List<Rol> Roles { get; set; } = new();
}
