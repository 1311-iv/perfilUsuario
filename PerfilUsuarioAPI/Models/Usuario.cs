using System.Collections.Generic;

namespace PerfilUsuarioAPI.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool Suspendido { get; set; }
        public List<Rol> Roles { get; set; } = new List<Rol>();
    }
}
