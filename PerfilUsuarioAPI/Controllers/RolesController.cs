using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Net;
using System.Web.Http;
using Dapper;
using PerfilUsuarioAPI.Models;

namespace PerfilUsuarioAPI.Controllers
{
    [RoutePrefix("api/roles")]
    public class RolesController : ApiController
    {
        private readonly string _connectionString =
            ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAll(string nombre = null)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    if (!string.IsNullOrWhiteSpace(nombre))
                    {
                        var roles = connection.Query<Rol>(
                            "SELECT id, strValor, strDescripcion FROM roles WHERE strValor LIKE @Nombre",
                            new { Nombre = "%" + nombre + "%" });
                        return Ok(roles);
                    }
                    else
                    {
                        var roles = connection.Query<Rol>(
                            "SELECT id, strValor, strDescripcion FROM roles");
                        return Ok(roles);
                    }
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
                    var rol = connection.QueryFirstOrDefault<Rol>(
                        "SELECT id, strValor, strDescripcion FROM roles WHERE id = @Id", new { Id = id });

                    if (rol == null)
                        return Content(HttpStatusCode.NotFound, new { message = "Rol no encontrado." });

                    return Ok(rol);
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult Create(Rol rol)
        {
            if (string.IsNullOrWhiteSpace(rol?.StrValor))
                return BadRequest("El valor del rol es requerido.");

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var id = connection.QuerySingle<int>(
                        @"INSERT INTO roles (strValor, strDescripcion) VALUES (@StrValor, @StrDescripcion);
                          SELECT CAST(SCOPE_IDENTITY() AS INT);",
                        rol);

                    return Content(HttpStatusCode.Created,
                        new { id, message = "Rol creado exitosamente." });
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPut]
        [Route("{id:int}")]
        public IHttpActionResult Update(int id, Rol rol)
        {
            if (string.IsNullOrWhiteSpace(rol?.StrValor))
                return BadRequest("El valor del rol es requerido.");

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var exists = connection.QueryFirstOrDefault<int?>(
                        "SELECT id FROM roles WHERE id = @Id", new { Id = id });

                    if (exists == null)
                        return Content(HttpStatusCode.NotFound, new { message = "Rol no encontrado." });

                    connection.Execute(
                        "UPDATE roles SET strValor = @StrValor, strDescripcion = @StrDescripcion WHERE id = @Id",
                        new { Id = id, rol.StrValor, rol.StrDescripcion });

                    return Ok(new { message = "Rol actualizado exitosamente." });
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
                                "SELECT id FROM roles WHERE id = @Id", new { Id = id }, transaction);

                            if (exists == null)
                            {
                                transaction.Rollback();
                                return Content(HttpStatusCode.NotFound, new { message = "Rol no encontrado." });
                            }

                            connection.Execute("DELETE FROM UsuarioRoles WHERE idRol = @Id", new { Id = id }, transaction);
                            connection.Execute("DELETE FROM roles WHERE id = @Id", new { Id = id }, transaction);

                            transaction.Commit();
                            return Ok(new { message = "Rol eliminado exitosamente." });
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
