namespace PerfilUsuarioAPI.Models;

public class Telefono
{
    public int Id { get; set; }
    public string Celular { get; set; } = string.Empty;
    public string Casa { get; set; } = string.Empty;
    public string Oficina { get; set; } = string.Empty;
    public int IdPerfilUsuario { get; set; }
}
