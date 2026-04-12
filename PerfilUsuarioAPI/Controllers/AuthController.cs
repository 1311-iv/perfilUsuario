using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Http;
using Dapper;
using PerfilUsuarioAPI.Models;

namespace PerfilUsuarioAPI.Controllers
{
    [RoutePrefix("api/auth")]
    public class AuthController : ApiController
    {
        private readonly string _connectionString =
            ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        [HttpPost]
        [Route("login")]
        public IHttpActionResult Login(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Username) || string.IsNullOrWhiteSpace(request?.Password))
                return BadRequest("Username y password son requeridos.");

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var usuario = connection.QueryFirstOrDefault<Usuario>(
                        "SELECT id, username, password, suspendido FROM usuarios WHERE username = @Username",
                        new { request.Username });

                    if (usuario == null)
                        return Content(System.Net.HttpStatusCode.Unauthorized,
                            new { message = "Usuario o contraseña incorrectos." });

                    if (usuario.Password != request.Password)
                        return Content(System.Net.HttpStatusCode.Unauthorized,
                            new { message = "Usuario o contraseña incorrectos." });

                    if (usuario.Suspendido)
                        return Content(System.Net.HttpStatusCode.Unauthorized,
                            new { message = "El usuario se encuentra suspendido." });

                    return Ok(new
                    {
                        message = "Login exitoso.",
                        usuario = new { usuario.Id, usuario.Username }
                    });
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
