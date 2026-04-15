using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web.Http;
using Dapper;
using PerfilUsuarioAPI.Models;

namespace PerfilUsuarioAPI.Controllers
{
    [RoutePrefix("api/perfilUsuario")]
    public class PerfilUsuarioController : ApiController
    {
        private readonly string _connectionString =
            ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAll()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var perfiles = connection.Query<PerfilUsuario>(
                        @"SELECT id, nombre, apellidoPaterno, apellidoMaterno, fechaNacimiento, rfc, idUsuario
                          FROM perfilUsuario
                          ORDER BY fechaNacimiento ASC").ToList();

                    foreach (var perfil in perfiles)
                    {
                        perfil.Telefono = connection.QueryFirstOrDefault<Telefono>(
                            "SELECT id, celular, casa, oficina, idPerfilUsuario FROM Telefonos WHERE idPerfilUsuario = @Id",
                            new { perfil.Id });

                        perfil.Direccion = connection.QueryFirstOrDefault<Direccion>(
                            "SELECT id, calle, colonia, NumInterior, NumExterior, Municipio, idPerfilUsuario FROM direcciones WHERE idPerfilUsuario = @Id",
                            new { perfil.Id });
                    }

                    return Ok(perfiles);
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult GetById(int id)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var perfil = connection.QueryFirstOrDefault<PerfilUsuario>(
                        @"SELECT id, nombre, apellidoPaterno, apellidoMaterno, fechaNacimiento, rfc, idUsuario
                          FROM perfilUsuario WHERE id = @Id", new { Id = id });

                    if (perfil == null)
                        return Content(HttpStatusCode.NotFound, new { message = "Perfil no encontrado." });

                    perfil.Telefono = connection.QueryFirstOrDefault<Telefono>(
                        "SELECT id, celular, casa, oficina, idPerfilUsuario FROM Telefonos WHERE idPerfilUsuario = @Id",
                        new { perfil.Id });

                    perfil.Direccion = connection.QueryFirstOrDefault<Direccion>(
                        "SELECT id, calle, colonia, NumInterior, NumExterior, Municipio, idPerfilUsuario FROM direcciones WHERE idPerfilUsuario = @Id",
                        new { perfil.Id });

                    return Ok(perfil);
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult Create(PerfilUsuarioRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Nombre) || string.IsNullOrWhiteSpace(request?.ApellidoPaterno))
                return BadRequest("Nombre y apellido paterno son requeridos.");

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            var perfilId = connection.QuerySingle<int>(
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
                                connection.Execute(
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
                                connection.Execute(
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
                            return Content(HttpStatusCode.Created,
                                new { id = perfilId, message = "Perfil creado exitosamente." });
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPut]
        [Route("{id:int}")]
        public IHttpActionResult Update(int id, PerfilUsuarioRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Nombre) || string.IsNullOrWhiteSpace(request?.ApellidoPaterno))
                return BadRequest("Nombre y apellido paterno son requeridos.");

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            var exists = connection.QueryFirstOrDefault<int?>(
                                "SELECT id FROM perfilUsuario WHERE id = @Id", new { Id = id }, transaction);

                            if (exists == null)
                            {
                                transaction.Rollback();
                                return Content(HttpStatusCode.NotFound, new { message = "Perfil no encontrado." });
                            }

                            connection.Execute(
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
                                var telExists = connection.QueryFirstOrDefault<int?>(
                                    "SELECT id FROM Telefonos WHERE idPerfilUsuario = @Id", new { Id = id }, transaction);

                                if (telExists != null)
                                {
                                    connection.Execute(
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
                                    connection.Execute(
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
                                var dirExists = connection.QueryFirstOrDefault<int?>(
                                    "SELECT id FROM direcciones WHERE idPerfilUsuario = @Id", new { Id = id }, transaction);

                                if (dirExists != null)
                                {
                                    connection.Execute(
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
                                    connection.Execute(
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
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpDelete]
        [Route("{id:int}")]
        public IHttpActionResult Delete(int id)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            var exists = connection.QueryFirstOrDefault<int?>(
                                "SELECT id FROM perfilUsuario WHERE id = @Id", new { Id = id }, transaction);

                            if (exists == null)
                            {
                                transaction.Rollback();
                                return Content(HttpStatusCode.NotFound, new { message = "Perfil no encontrado." });
                            }

                            var totalDirecciones = connection.ExecuteScalar<int>(
                                "SELECT COUNT(*) FROM direcciones WHERE idPerfilUsuario = @Id",
                                new { Id = id }, transaction);

                            if (totalDirecciones > 1)
                            {
                                transaction.Rollback();
                                return Content(HttpStatusCode.BadRequest,
                                    new { message = "No se puede eliminar el perfil porque tiene más de 1 dirección. Elimine direcciones hasta dejar máximo 1." });
                            }

                            connection.Execute(
                                "DELETE FROM Telefonos WHERE idPerfilUsuario = @Id", new { Id = id }, transaction);

                            connection.Execute(
                                "DELETE FROM direcciones WHERE idPerfilUsuario = @Id", new { Id = id }, transaction);

                            connection.Execute(
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
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
