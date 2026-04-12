using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using PerfilUsuarioAPI.Models;

namespace PerfilUsuarioAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly string _connectionString;

    public AuthController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "Username y password son requeridos." });

        try
        {
            using var connection = new SqlConnection(_connectionString);
            var usuario = await connection.QueryFirstOrDefaultAsync<Usuario>(
                "SELECT id, username, password, suspendido FROM usuarios WHERE username = @Username",
                new { request.Username });

            if (usuario == null)
                return Unauthorized(new { message = "Usuario o contraseña incorrectos." });

            if (usuario.Password != request.Password)
                return Unauthorized(new { message = "Usuario o contraseña incorrectos." });

            if (usuario.Suspendido)
                return Unauthorized(new { message = "El usuario se encuentra suspendido." });

            return Ok(new
            {
                message = "Login exitoso.",
                usuario = new { usuario.Id, usuario.Username }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor.", error = ex.Message });
        }
    }
}
