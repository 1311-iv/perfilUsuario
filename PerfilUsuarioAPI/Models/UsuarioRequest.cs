using System.Collections.Generic;

namespace PerfilUsuarioAPI.Models
{
    public class UsuarioRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public bool Suspendido { get; set; }
        public List<int> RolIds { get; set; } = new List<int>();
    }
}
