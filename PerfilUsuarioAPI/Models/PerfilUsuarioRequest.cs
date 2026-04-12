using System;

namespace PerfilUsuarioAPI.Models
{
    public class PerfilUsuarioRequest
    {
        public string Nombre { get; set; }
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string Rfc { get; set; }
        public int IdUsuario { get; set; }
        public TelefonoRequest Telefono { get; set; }
        public DireccionRequest Direccion { get; set; }
    }

    public class TelefonoRequest
    {
        public string Celular { get; set; }
        public string Casa { get; set; }
        public string Oficina { get; set; }
    }

    public class DireccionRequest
    {
        public string Calle { get; set; }
        public string Colonia { get; set; }
        public string NumInterior { get; set; }
        public string NumExterior { get; set; }
        public string Municipio { get; set; }
    }
}
