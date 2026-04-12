using System;

namespace PerfilUsuarioAPI.Models
{
    public class PerfilUsuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string Rfc { get; set; }
        public int IdUsuario { get; set; }
        public Telefono Telefono { get; set; }
        public Direccion Direccion { get; set; }
    }
}
