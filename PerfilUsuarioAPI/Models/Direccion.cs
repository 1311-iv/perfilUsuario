namespace PerfilUsuarioAPI.Models;

public class Direccion
{
    public int Id { get; set; }
    public string Calle { get; set; } = string.Empty;
    public string Colonia { get; set; } = string.Empty;
    public string NumInterior { get; set; } = string.Empty;
    public string NumExterior { get; set; } = string.Empty;
    public string Municipio { get; set; } = string.Empty;
    public int IdPerfilUsuario { get; set; }
}
