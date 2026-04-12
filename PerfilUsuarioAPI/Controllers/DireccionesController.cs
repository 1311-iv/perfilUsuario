using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using PerfilUsuarioAPI.Models;

namespace PerfilUsuarioAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DireccionesController : ControllerBase
{
    private readonly string _connectionString;

    public DireccionesController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            var direcciones = await connection.QueryAsync<Direccion>(
                "SELECT id, calle, colonia, NumInterior, NumExterior, Municipio, idPerfilUsuario FROM direcciones");
            return Ok(direcciones);
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
            var direccion = await connection.QueryFirstOrDefaultAsync<Direccion>(
                "SELECT id, calle, colonia, NumInterior, NumExterior, Municipio, idPerfilUsuario FROM direcciones WHERE id = @Id",
                new { Id = id });

            if (direccion == null)
                return NotFound(new { message = "Dirección no encontrada." });

            return Ok(direccion);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor.", error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Direccion direccion)
    {
        if (string.IsNullOrWhiteSpace(direccion.Calle))
            return BadRequest(new { message = "La calle es requerida." });

        try
        {
            using var connection = new SqlConnection(_connectionString);
            var id = await connection.QuerySingleAsync<int>(
                @"INSERT INTO direcciones (calle, colonia, NumInterior, NumExterior, Municipio, idPerfilUsuario)
                  VALUES (@Calle, @Colonia, @NumInterior, @NumExterior, @Municipio, @IdPerfilUsuario);
                  SELECT CAST(SCOPE_IDENTITY() AS INT);",
                direccion);

            return CreatedAtAction(nameof(GetById), new { id }, new { id, message = "Dirección creada exitosamente." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor.", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Direccion direccion)
    {
        if (string.IsNullOrWhiteSpace(direccion.Calle))
            return BadRequest(new { message = "La calle es requerida." });

        try
        {
            using var connection = new SqlConnection(_connectionString);
            var exists = await connection.QueryFirstOrDefaultAsync<int?>(
                "SELECT id FROM direcciones WHERE id = @Id", new { Id = id });

            if (exists == null)
                return NotFound(new { message = "Dirección no encontrada." });

            await connection.ExecuteAsync(
                @"UPDATE direcciones SET calle = @Calle, colonia = @Colonia, NumInterior = @NumInterior,
                  NumExterior = @NumExterior, Municipio = @Municipio, idPerfilUsuario = @IdPerfilUsuario
                  WHERE id = @Id",
                new
                {
                    Id = id,
                    direccion.Calle,
                    direccion.Colonia,
                    direccion.NumInterior,
                    direccion.NumExterior,
                    direccion.Municipio,
                    direccion.IdPerfilUsuario
                });

            return Ok(new { message = "Dirección actualizada exitosamente." });
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
                "SELECT id FROM direcciones WHERE id = @Id", new { Id = id });

            if (exists == null)
                return NotFound(new { message = "Dirección no encontrada." });

            await connection.ExecuteAsync("DELETE FROM direcciones WHERE id = @Id", new { Id = id });
            return Ok(new { message = "Dirección eliminada exitosamente." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor.", error = ex.Message });
        }
    }
}
