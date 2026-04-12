using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using PerfilUsuarioAPI.Models;

namespace PerfilUsuarioAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly string _connectionString;

    public UsuariosController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            var usuarios = (await connection.QueryAsync<Usuario>(
                "SELECT id, username, password, suspendido FROM usuarios")).ToList();

            foreach (var usuario in usuarios)
            {
                usuario.Roles = (await connection.QueryAsync<Rol>(
                    @"SELECT r.id, r.strValor, r.strDescripcion
                      FROM roles r
                      INNER JOIN UsuarioRoles ur ON ur.idRol = r.id
                      WHERE ur.idUsuario = @IdUsuario",
                    new { IdUsuario = usuario.Id })).ToList();
            }

            return Ok(usuarios);
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
            var usuario = await connection.QueryFirstOrDefaultAsync<Usuario>(
                "SELECT id, username, password, suspendido FROM usuarios WHERE id = @Id",
                new { Id = id });

            if (usuario == null)
                return NotFound(new { message = "Usuario no encontrado." });

            usuario.Roles = (await connection.QueryAsync<Rol>(
                @"SELECT r.id, r.strValor, r.strDescripcion
                  FROM roles r
                  INNER JOIN UsuarioRoles ur ON ur.idRol = r.id
                  WHERE ur.idUsuario = @IdUsuario",
                new { IdUsuario = usuario.Id })).ToList();

            return Ok(usuario);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor.", error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UsuarioRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "Username y password son requeridos." });

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                var userId = await connection.QuerySingleAsync<int>(
                    @"INSERT INTO usuarios (username, password, suspendido)
                      VALUES (@Username, @Password, @Suspendido);
                      SELECT CAST(SCOPE_IDENTITY() AS INT);",
                    new { request.Username, request.Password, request.Suspendido }, transaction);

                foreach (var rolId in request.RolIds)
                {
                    await connection.ExecuteAsync(
                        "INSERT INTO UsuarioRoles (idUsuario, idRol) VALUES (@IdUsuario, @IdRol)",
                        new { IdUsuario = userId, IdRol = rolId }, transaction);
                }

                transaction.Commit();
                return CreatedAtAction(nameof(GetById), new { id = userId }, new { id = userId, message = "Usuario creado exitosamente." });
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor.", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UsuarioRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "Username y password son requeridos." });

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                var exists = await connection.QueryFirstOrDefaultAsync<int?>(
                    "SELECT id FROM usuarios WHERE id = @Id", new { Id = id }, transaction);

                if (exists == null)
                {
                    transaction.Rollback();
                    return NotFound(new { message = "Usuario no encontrado." });
                }

                await connection.ExecuteAsync(
                    "UPDATE usuarios SET username = @Username, password = @Password, suspendido = @Suspendido WHERE id = @Id",
                    new { Id = id, request.Username, request.Password, request.Suspendido }, transaction);

                await connection.ExecuteAsync(
                    "DELETE FROM UsuarioRoles WHERE idUsuario = @Id", new { Id = id }, transaction);

                foreach (var rolId in request.RolIds)
                {
                    await connection.ExecuteAsync(
                        "INSERT INTO UsuarioRoles (idUsuario, idRol) VALUES (@IdUsuario, @IdRol)",
                        new { IdUsuario = id, IdRol = rolId }, transaction);
                }

                transaction.Commit();
                return Ok(new { message = "Usuario actualizado exitosamente." });
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
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
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                var exists = await connection.QueryFirstOrDefaultAsync<int?>(
                    "SELECT id FROM usuarios WHERE id = @Id", new { Id = id }, transaction);

                if (exists == null)
                {
                    transaction.Rollback();
                    return NotFound(new { message = "Usuario no encontrado." });
                }

                await connection.ExecuteAsync(
                    "DELETE FROM UsuarioRoles WHERE idUsuario = @Id", new { Id = id }, transaction);

                await connection.ExecuteAsync(
                    "DELETE FROM usuarios WHERE id = @Id", new { Id = id }, transaction);

                transaction.Commit();
                return Ok(new { message = "Usuario eliminado exitosamente." });
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor.", error = ex.Message });
        }
    }
}
