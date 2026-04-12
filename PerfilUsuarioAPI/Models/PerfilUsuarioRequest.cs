namespace PerfilUsuarioAPI.Models;

public class PerfilUsuarioRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string ApellidoPaterno { get; set; } = string.Empty;
    public string ApellidoMaterno { get; set; } = string.Empty;
    public DateTime? FechaNacimiento { get; set; }
    public string Rfc { get; set; } = string.Empty;
    public int IdUsuario { get; set; }
    public TelefonoRequest? Telefono { get; set; }
    public DireccionRequest? Direccion { get; set; }
}

public class TelefonoRequest
{
    public string Celular { get; set; } = string.Empty;
    public string Casa { get; set; } = string.Empty;
    public string Oficina { get; set; } = string.Empty;
}

public class DireccionRequest
{
    public string Calle { get; set; } = string.Empty;
    public string Colonia { get; set; } = string.Empty;
    public string NumInterior { get; set; } = string.Empty;
    public string NumExterior { get; set; } = string.Empty;
    public string Municipio { get; set; } = string.Empty;
}
