using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using PerfilUsuarioAPI.Models;

namespace PerfilUsuarioAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PerfilUsuarioController : ControllerBase
{
    private readonly string _connectionString;

    public PerfilUsuarioController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            var perfiles = (await connection.QueryAsync<PerfilUsuario>(
                @"SELECT id, nombre, apellidoPaterno, apellidoMaterno, fechaNacimiento, rfc, idUsuario
                  FROM perfilUsuario
                  ORDER BY fechaNacimiento ASC")).ToList();

            foreach (var perfil in perfiles)
            {
                perfil.Telefono = await connection.QueryFirstOrDefaultAsync<Telefono>(
                    "SELECT id, celular, casa, oficina, idPerfilUsuario FROM Telefonos WHERE idPerfilUsuario = @Id",
                    new { perfil.Id });

                perfil.Direccion = await connection.QueryFirstOrDefaultAsync<Direccion>(
                    "SELECT id, calle, colonia, NumInterior, NumExterior, Municipio, idPerfilUsuario FROM direcciones WHERE idPerfilUsuario = @Id",
                    new { perfil.Id });
            }

            return Ok(perfiles);
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
            var perfil = await connection.QueryFirstOrDefaultAsync<PerfilUsuario>(
                @"SELECT id, nombre, apellidoPaterno, apellidoMaterno, fechaNacimiento, rfc, idUsuario
                  FROM perfilUsuario WHERE id = @Id", new { Id = id });

            if (perfil == null)
                return NotFound(new { message = "Perfil no encontrado." });

            perfil.Telefono = await connection.QueryFirstOrDefaultAsync<Telefono>(
                "SELECT id, celular, casa, oficina, idPerfilUsuario FROM Telefonos WHERE idPerfilUsuario = @Id",
                new { perfil.Id });

            perfil.Direccion = await connection.QueryFirstOrDefaultAsync<Direccion>(
                "SELECT id, calle, colonia, NumInterior, NumExterior, Municipio, idPerfilUsuario FROM direcciones WHERE idPerfilUsuario = @Id",
                new { perfil.Id });

            return Ok(perfil);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor.", error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PerfilUsuarioRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre) || string.IsNullOrWhiteSpace(request.ApellidoPaterno))
            return BadRequest(new { message = "Nombre y apellido paterno son requeridos." });

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                var perfilId = await connection.QuerySingleAsync<int>(
                    @"INSERT INTO perfilUsuario (nombre, apellidoPaterno, apellidoMaterno, fechaNacimiento, rfc, idUsuario)
                      VALUES (@Nombre, @ApellidoPaterno, @ApellidoMaterno, @FechaNacimiento, @Rfc, @IdUsuario);
                      SELECT CAST(SCOPE_IDENTITY() AS INT);",
                    new
                    {
                        request.Nombre,
                        request.ApellidoPaterno,
                        request.ApellidoMaterno,
                        request.FechaNacimiento,
                        request.Rfc,
                        request.IdUsuario
                    }, transaction);

                if (request.Telefono != null)
                {
                    await connection.ExecuteAsync(
                        @"INSERT INTO Telefonos (celular, casa, oficina, idPerfilUsuario)
                          VALUES (@Celular, @Casa, @Oficina, @IdPerfilUsuario)",
                        new
                        {
                            request.Telefono.Celular,
                            request.Telefono.Casa,
                            request.Telefono.Oficina,
                            IdPerfilUsuario = perfilId
                        }, transaction);
                }

                if (request.Direccion != null)
                {
                    await connection.ExecuteAsync(
                        @"INSERT INTO direcciones (calle, colonia, NumInterior, NumExterior, Municipio, idPerfilUsuario)
                          VALUES (@Calle, @Colonia, @NumInterior, @NumExterior, @Municipio, @IdPerfilUsuario)",
                        new
                        {
                            request.Direccion.Calle,
                            request.Direccion.Colonia,
                            request.Direccion.NumInterior,
                            request.Direccion.NumExterior,
                            request.Direccion.Municipio,
                            IdPerfilUsuario = perfilId
                        }, transaction);
                }

                transaction.Commit();
                return CreatedAtAction(nameof(GetById), new { id = perfilId }, new { id = perfilId, message = "Perfil creado exitosamente." });
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
    public async Task<IActionResult> Update(int id, [FromBody] PerfilUsuarioRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre) || string.IsNullOrWhiteSpace(request.ApellidoPaterno))
            return BadRequest(new { message = "Nombre y apellido paterno son requeridos." });

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                var exists = await connection.QueryFirstOrDefaultAsync<int?>(
                    "SELECT id FROM perfilUsuario WHERE id = @Id", new { Id = id }, transaction);

                if (exists == null)
                {
                    transaction.Rollback();
                    return NotFound(new { message = "Perfil no encontrado." });
                }

                await connection.ExecuteAsync(
                    @"UPDATE perfilUsuario SET nombre = @Nombre, apellidoPaterno = @ApellidoPaterno,
                      apellidoMaterno = @ApellidoMaterno, fechaNacimiento = @FechaNacimiento,
                      rfc = @Rfc, idUsuario = @IdUsuario WHERE id = @Id",
                    new
                    {
                        Id = id,
                        request.Nombre,
                        request.ApellidoPaterno,
                        request.ApellidoMaterno,
                        request.FechaNacimiento,
                        request.Rfc,
                        request.IdUsuario
                    }, transaction);

                if (request.Telefono != null)
                {
                    var telExists = await connection.QueryFirstOrDefaultAsync<int?>(
                        "SELECT id FROM Telefonos WHERE idPerfilUsuario = @Id", new { Id = id }, transaction);

                    if (telExists != null)
                    {
                        await connection.ExecuteAsync(
                            @"UPDATE Telefonos SET celular = @Celular, casa = @Casa, oficina = @Oficina
                              WHERE idPerfilUsuario = @IdPerfilUsuario",
                            new
                            {
                                request.Telefono.Celular,
                                request.Telefono.Casa,
                                request.Telefono.Oficina,
                                IdPerfilUsuario = id
                            }, transaction);
                    }
                    else
                    {
                        await connection.ExecuteAsync(
                            @"INSERT INTO Telefonos (celular, casa, oficina, idPerfilUsuario)
                              VALUES (@Celular, @Casa, @Oficina, @IdPerfilUsuario)",
                            new
                            {
                                request.Telefono.Celular,
                                request.Telefono.Casa,
                                request.Telefono.Oficina,
                                IdPerfilUsuario = id
                            }, transaction);
                    }
                }

                if (request.Direccion != null)
                {
                    var dirExists = await connection.QueryFirstOrDefaultAsync<int?>(
                        "SELECT id FROM direcciones WHERE idPerfilUsuario = @Id", new { Id = id }, transaction);

                    if (dirExists != null)
                    {
                        await connection.ExecuteAsync(
                            @"UPDATE direcciones SET calle = @Calle, colonia = @Colonia, NumInterior = @NumInterior,
                              NumExterior = @NumExterior, Municipio = @Municipio WHERE idPerfilUsuario = @IdPerfilUsuario",
                            new
                            {
                                request.Direccion.Calle,
                                request.Direccion.Colonia,
                                request.Direccion.NumInterior,
                                request.Direccion.NumExterior,
                                request.Direccion.Municipio,
                                IdPerfilUsuario = id
                            }, transaction);
                    }
                    else
                    {
                        await connection.ExecuteAsync(
                            @"INSERT INTO direcciones (calle, colonia, NumInterior, NumExterior, Municipio, idPerfilUsuario)
                              VALUES (@Calle, @Colonia, @NumInterior, @NumExterior, @Municipio, @IdPerfilUsuario)",
                            new
                            {
                                request.Direccion.Calle,
                                request.Direccion.Colonia,
                                request.Direccion.NumInterior,
                                request.Direccion.NumExterior,
                                request.Direccion.Municipio,
                                IdPerfilUsuario = id
                            }, transaction);
                    }
                }

                transaction.Commit();
                return Ok(new { message = "Perfil actualizado exitosamente." });
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
                    "SELECT id FROM perfilUsuario WHERE id = @Id", new { Id = id }, transaction);

                if (exists == null)
                {
                    transaction.Rollback();
                    return NotFound(new { message = "Perfil no encontrado." });
                }

                await connection.ExecuteAsync(
                    "DELETE FROM Telefonos WHERE idPerfilUsuario = @Id", new { Id = id }, transaction);

                await connection.ExecuteAsync(
                    "DELETE FROM direcciones WHERE idPerfilUsuario = @Id", new { Id = id }, transaction);

                await connection.ExecuteAsync(
                    "DELETE FROM perfilUsuario WHERE id = @Id", new { Id = id }, transaction);

                transaction.Commit();
                return Ok(new { message = "Perfil eliminado exitosamente." });
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
