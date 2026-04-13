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
    [RoutePrefix("api/usuarios")]
    public class UsuariosController : ApiController
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
                    var usuarios = connection.Query<Usuario>(
                        "SELECT id, username, password, suspendido FROM usuarios").ToList();

                    foreach (var usuario in usuarios)
                    {
                        usuario.Roles = connection.Query<Rol>(
                            @"SELECT r.id, r.strValor, r.strDescripcion
                              FROM roles r
                              INNER JOIN UsuarioRoles ur ON ur.idRol = r.id
                              WHERE ur.idUsuario = @IdUsuario",
                            new { IdUsuario = usuario.Id }).ToList();
                    }

                    var result = usuarios.Select(u => new { u.Id, u.Username, u.Suspendido, u.Roles });
                    return Ok(result);
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
                    var usuario = connection.QueryFirstOrDefault<Usuario>(
                        "SELECT id, username, password, suspendido FROM usuarios WHERE id = @Id",
                        new { Id = id });

                    if (usuario == null)
                        return Content(HttpStatusCode.NotFound, new { message = "Usuario no encontrado." });

                    usuario.Roles = connection.Query<Rol>(
                        @"SELECT r.id, r.strValor, r.strDescripcion
                          FROM roles r
                          INNER JOIN UsuarioRoles ur ON ur.idRol = r.id
                          WHERE ur.idUsuario = @IdUsuario",
                        new { IdUsuario = usuario.Id }).ToList();

                    return Ok(new { usuario.Id, usuario.Username, usuario.Suspendido, usuario.Roles });
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult Create(UsuarioRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Username) || string.IsNullOrWhiteSpace(request?.Password))
                return BadRequest("Username y password son requeridos.");

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            var existing = connection.QueryFirstOrDefault<int?>(
                                "SELECT id FROM usuarios WHERE username = @Username",
                                new { request.Username }, transaction);

                            if (existing != null)
                            {
                                transaction.Rollback();
                                return Content(HttpStatusCode.BadRequest,
                                    new { message = "El usuario ya existe." });
                            }

                            var userId = connection.QuerySingle<int>(
                                @"INSERT INTO usuarios (username, password, suspendido)
                                  VALUES (@Username, @Password, @Suspendido);
                                  SELECT CAST(SCOPE_IDENTITY() AS INT);",
                                new { request.Username, request.Password, request.Suspendido }, transaction);

                            foreach (var rolId in request.RolIds)
                            {
                                connection.Execute(
                                    "INSERT INTO UsuarioRoles (idUsuario, idRol) VALUES (@IdUsuario, @IdRol)",
                                    new { IdUsuario = userId, IdRol = rolId }, transaction);
                            }

                            transaction.Commit();
                            return Content(HttpStatusCode.Created,
                                new { id = userId, message = "Usuario creado exitosamente." });
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
        public IHttpActionResult Update(int id, UsuarioRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Username) || string.IsNullOrWhiteSpace(request?.Password))
                return BadRequest("Username y password son requeridos.");

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
                                "SELECT id FROM usuarios WHERE id = @Id", new { Id = id }, transaction);

                            if (exists == null)
                            {
                                transaction.Rollback();
                                return Content(HttpStatusCode.NotFound, new { message = "Usuario no encontrado." });
                            }

                            connection.Execute(
                                "UPDATE usuarios SET username = @Username, password = @Password, suspendido = @Suspendido WHERE id = @Id",
                                new { Id = id, request.Username, request.Password, request.Suspendido }, transaction);

                            connection.Execute(
                                "DELETE FROM UsuarioRoles WHERE idUsuario = @Id", new { Id = id }, transaction);

                            foreach (var rolId in request.RolIds)
                            {
                                connection.Execute(
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
                                "SELECT id FROM usuarios WHERE id = @Id", new { Id = id }, transaction);

                            if (exists == null)
                            {
                                transaction.Rollback();
                                return Content(HttpStatusCode.NotFound, new { message = "Usuario no encontrado." });
                            }

                            connection.Execute(
                                "DELETE FROM UsuarioRoles WHERE idUsuario = @Id", new { Id = id }, transaction);

                            var perfilIds = connection.Query<int>(
                                "SELECT id FROM perfilUsuario WHERE idUsuario = @Id", new { Id = id }, transaction).ToList();

                            foreach (var perfilId in perfilIds)
                            {
                                connection.Execute(
                                    "DELETE FROM Telefonos WHERE idPerfilUsuario = @PerfilId", new { PerfilId = perfilId }, transaction);
                                connection.Execute(
                                    "DELETE FROM direcciones WHERE idPerfilUsuario = @PerfilId", new { PerfilId = perfilId }, transaction);
                            }

                            connection.Execute(
                                "DELETE FROM perfilUsuario WHERE idUsuario = @Id", new { Id = id }, transaction);

                            connection.Execute(
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
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
