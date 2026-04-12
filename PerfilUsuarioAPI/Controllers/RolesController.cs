using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using PerfilUsuarioAPI.Models;

namespace PerfilUsuarioAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly string _connectionString;

    public RolesController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? nombre = null)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);

            if (!string.IsNullOrWhiteSpace(nombre))
            {
                var roles = await connection.QueryAsync<Rol>(
                    "SELECT id, strValor, strDescripcion FROM roles WHERE strValor LIKE @Nombre",
                    new { Nombre = $"%{nombre}%" });
                return Ok(roles);
            }
            else
            {
                var roles = await connection.QueryAsync<Rol>(
                    "SELECT id, strValor, strDescripcion FROM roles");
                return Ok(roles);
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor.", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            var rol = await connection.QueryFirstOrDefaultAsync<Rol>(
                "SELECT id, strValor, strDescripcion FROM roles WHERE id = @Id", new { Id = id });

            if (rol == null)
                return NotFound(new { message = "Rol no encontrado." });

            return Ok(rol);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor.", error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Rol rol)
    {
        if (string.IsNullOrWhiteSpace(rol.StrValor))
            return BadRequest(new { message = "El valor del rol es requerido." });

        try
        {
            using var connection = new SqlConnection(_connectionString);
            var id = await connection.QuerySingleAsync<int>(
                @"INSERT INTO roles (strValor, strDescripcion) VALUES (@StrValor, @StrDescripcion);
                  SELECT CAST(SCOPE_IDENTITY() AS INT);",
                rol);

            return CreatedAtAction(nameof(GetById), new { id }, new { id, message = "Rol creado exitosamente." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor.", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Rol rol)
    {
        if (string.IsNullOrWhiteSpace(rol.StrValor))
            return BadRequest(new { message = "El valor del rol es requerido." });

        try
        {
            using var connection = new SqlConnection(_connectionString);
            var exists = await connection.QueryFirstOrDefaultAsync<int?>(
                "SELECT id FROM roles WHERE id = @Id", new { Id = id });

            if (exists == null)
                return NotFound(new { message = "Rol no encontrado." });

            await connection.ExecuteAsync(
                "UPDATE roles SET strValor = @StrValor, strDescripcion = @StrDescripcion WHERE id = @Id",
                new { Id = id, rol.StrValor, rol.StrDescripcion });

            return Ok(new { message = "Rol actualizado exitosamente." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor.", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            var exists = await connection.QueryFirstOrDefaultAsync<int?>(
                "SELECT id FROM roles WHERE id = @Id", new { Id = id });

            if (exists == null)
                return NotFound(new { message = "Rol no encontrado." });

            await connection.ExecuteAsync("DELETE FROM UsuarioRoles WHERE idRol = @Id", new { Id = id });
            await connection.ExecuteAsync("DELETE FROM roles WHERE id = @Id", new { Id = id });
            return Ok(new { message = "Rol eliminado exitosamente." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor.", error = ex.Message });
        }
    }
}
