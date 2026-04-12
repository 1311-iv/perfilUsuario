namespace PerfilUsuarioAPI.Models;

public class PerfilUsuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string ApellidoPaterno { get; set; } = string.Empty;
    public string ApellidoMaterno { get; set; } = string.Empty;
    public DateTime? FechaNacimiento { get; set; }
    public string Rfc { get; set; } = string.Empty;
    public int IdUsuario { get; set; }
    public Telefono? Telefono { get; set; }
    public Direccion? Direccion { get; set; }
}
