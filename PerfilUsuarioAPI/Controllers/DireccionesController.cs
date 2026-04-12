using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Net;
using System.Web.Http;
using Dapper;
using PerfilUsuarioAPI.Models;

namespace PerfilUsuarioAPI.Controllers
{
    [RoutePrefix("api/direcciones")]
    public class DireccionesController : ApiController
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
                    var direcciones = connection.Query<Direccion>(
                        "SELECT id, calle, colonia, NumInterior, NumExterior, Municipio, idPerfilUsuario FROM direcciones");
                    return Ok(direcciones);
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
                    var direccion = connection.QueryFirstOrDefault<Direccion>(
                        "SELECT id, calle, colonia, NumInterior, NumExterior, Municipio, idPerfilUsuario FROM direcciones WHERE id = @Id",
                        new { Id = id });

                    if (direccion == null)
                        return Content(HttpStatusCode.NotFound, new { message = "Dirección no encontrada." });

                    return Ok(direccion);
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult Create(Direccion direccion)
        {
            if (string.IsNullOrWhiteSpace(direccion?.Calle))
                return BadRequest("La calle es requerida.");

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var id = connection.QuerySingle<int>(
                        @"INSERT INTO direcciones (calle, colonia, NumInterior, NumExterior, Municipio, idPerfilUsuario)
                          VALUES (@Calle, @Colonia, @NumInterior, @NumExterior, @Municipio, @IdPerfilUsuario);
                          SELECT CAST(SCOPE_IDENTITY() AS INT);",
                        direccion);

                    return Content(HttpStatusCode.Created,
                        new { id, message = "Dirección creada exitosamente." });
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPut]
        [Route("{id:int}")]
        public IHttpActionResult Update(int id, Direccion direccion)
        {
            if (string.IsNullOrWhiteSpace(direccion?.Calle))
                return BadRequest("La calle es requerida.");

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var exists = connection.QueryFirstOrDefault<int?>(
                        "SELECT id FROM direcciones WHERE id = @Id", new { Id = id });

                    if (exists == null)
                        return Content(HttpStatusCode.NotFound, new { message = "Dirección no encontrada." });

                    connection.Execute(
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
                    var exists = connection.QueryFirstOrDefault<int?>(
                        "SELECT id FROM direcciones WHERE id = @Id", new { Id = id });

                    if (exists == null)
                        return Content(HttpStatusCode.NotFound, new { message = "Dirección no encontrada." });

                    connection.Execute("DELETE FROM direcciones WHERE id = @Id", new { Id = id });
                    return Ok(new { message = "Dirección eliminada exitosamente." });
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
