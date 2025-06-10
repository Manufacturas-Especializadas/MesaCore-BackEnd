using MesaCore.Dtos;
using MesaCore.Models;
using MesaCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace MesaCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImpresorasController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AzureStorageService _azureStorageService;
        private readonly string _contenedor = "filefai";

        public ImpresorasController(AppDbContext context, AzureStorageService azureStorageService)
        {
            _context = context;
            _azureStorageService = azureStorageService;
        }

        [HttpGet]
        [Route("Paginacion")]
        public async Task<IActionResult> GetPaginacion([FromQuery] int page = 1,
                                                        [FromQuery] int pageSize = 10,
                                                        [FromQuery] string codigo = null,
                                                        [FromQuery] string nParte = null)
        {
            try
            {
                var query = _context.Impresorasalfx.AsQueryable();

                if (!string.IsNullOrWhiteSpace(codigo))
                {
                    query = query.Where(p => p.Codigo.Contains(codigo));
                }

                if (!string.IsNullOrEmpty(nParte))
                {
                    query = query.Where(p => p.NParte.Contains(nParte));
                }

                var totalRecords = query.Count();

                var printers = await query
                               .OrderBy(p => p.Id)
                               .Select(i => new
                               {
                                   i.Id,
                                   NombreProyecto = i.Proyecto.NombreDelProyecto,
                                   i.Codigo,                                   
                                   ClienteId = i.Cliente.Nombre,
                                   i.NDibujo,
                                   i.NParte,
                                   i.Revision,
                                   i.EntregaLaboratorio,
                                   i.Fai,
                                   i.LiberacionLaboratorio,
                                   i.Comentarios,
                                   EstatusId = i.Estatus.Nombre
                               })
                               .Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .AsNoTracking()
                               .ToListAsync();

                return Ok(new { data = printers, totalRecords, page, pageSize });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("Obtener")]
        public async Task<IActionResult> GetImpresorasCobreAsync()
        {
            var proyectos = await _context.Proyectosfxsal
         .Where(p => p.NombreDelProyecto != null && p.Impresorasalfx.Any())
         .Select(p => new ProyectoCardDto
         {
             Id = p.Id,
             NombreDelProyecto = p.NombreDelProyecto,
             FechaSolicitud = p.FechaDeLaSolicitud,
             SolicitanteNombre = p.Solicitante.Nombre,
             PlantaNombre = p.Planta.Nombre,
             EstatusNombre = p.Estatus.Nombre,
             Impresiones = p.Impresorasalfx
                 .Where(i => i.EstatusId.HasValue)
                 .Select(i => new ImpresoraCardAlDto
                 {
                     Id = i.Id,
                     NParte = i.NParte,
                     NDibujo = i.NDibujo,
                     Revision = i.Revision,
                     EstatusNombre = i.Estatus.Nombre,
                     NombreDelProyecto = i.Proyecto.NombreDelProyecto,
                     Solicitante = i.Proyecto.Solicitante.Nombre
                 })
                 .ToList()
         })
         .ToListAsync();

            if (!proyectos.Any())
            {
                return Ok(new List<ProyectoCardDto>());
            }

            var result = proyectos
                .OrderBy(p => p.NombreDelProyecto)
                .ToList();

            return Ok(result);
        }

        [HttpGet]
        [Route("ObtenerImpresoraPorId/{id}")]
        public async Task<IActionResult> GetImpresoraPorIdAsync(int id)
        {
            var impresorId = await _context.Impresorasalfx.FirstOrDefaultAsync(i => i.Id == id);

            if (impresorId == null)
            {
                throw new Exception("No se encontro el id");
            }

            return Ok(impresorId);
        }

        [HttpGet]
        [Route("ObtenerImpresorasAluminioPorNombreProyecto/{nombreDelProyecto}")]
        public async Task<IActionResult> GetImpresorasPorNombreProyectoAsync(string nombreDelProyecto)
        {
            if (string.IsNullOrEmpty(nombreDelProyecto))
            {
                return BadRequest("EL nombre del proyecto no puede estar vacio");
            }

            var proyectoData = await _context.Proyectosfxsal
                .Where(p => p.NombreDelProyecto == nombreDelProyecto)
                .Select(p => new
                {
                    Proyecto = new
                    {
                        p.Id,
                        p.NombreDelProyecto,
                        p.FechaDeLaSolicitud,
                        EstatusNombre = p.Estatus.Nombre,
                        PlantaNombre = p.Planta.Nombre,
                        SolicitanteNombre = p.Solicitante.Nombre
                    },
                    Impresiones = p.Impresorasalfx.Select(i => new
                    {
                        i.Id,
                        i.NParte,
                        i.NDibujo,
                        i.Revision,
                        EstatusNombre = i.Estatus.Nombre,
                    }).ToList()
                }).FirstOrDefaultAsync();

            if(proyectoData == null)
            {
                return NotFound("No se encontró el proyecto");
            }

            var result = new
            {
                proyectoData.Proyecto.Id,
                proyectoData.Proyecto.NombreDelProyecto,
                proyectoData.Proyecto.FechaDeLaSolicitud,
                proyectoData.Proyecto.SolicitanteNombre,
                proyectoData.Proyecto.PlantaNombre,
                proyectoData.Proyecto.EstatusNombre,
                Impresiones = proyectoData.Impresiones
            };

            return Ok(result);
        }

        [HttpGet]
        [Route("ObtenerListaProyectosAL")]
        public async Task<List<Proyectosfxsal>> GetProyectosAlAsync()
        {
            var lista = await _context.Proyectosfxsal
                                        .AsNoTracking()
                                        .ToListAsync();

            if (lista is null)
            {
                throw new Exception("Error al obtener la información");
            }


            return lista;
        }

        [HttpGet]
        [Route("ObtenerListaEstatus")]
        public async Task<List<Estatusimpresorasfx>> GetEstatusimpresorasfxesAsync()
        {
            var lista = await _context.Estatusimpresorasfx
                                        .AsNoTracking()
                                        .ToListAsync();

            if (lista is null)
            {
                throw new Exception("Error al obtener la información");
            }


            return lista;
        }

        [HttpGet]
        [Route("ObtenerListaEstatusProyecto")]
        public async Task<List<Estatusproyectoimpresorasfx>> GetEstatusproyectoimpresorasfxesAsync()
        {
            var lista = await _context.Estatusproyectoimpresorasfx
                                        .AsNoTracking()
                                        .ToListAsync();

            if (lista is null)
            {
                throw new Exception("Error al obtener la información");
            }

            return lista;
        }

        [HttpGet]
        [Route("ObtenerListaPlanta")]
        public async Task<List<Plantaimpresorasfx>> GetPlantaimpresorasfxesAsync()
        {
            var lista = await _context.Plantaimpresorasfx
                                        .AsNoTracking()
                                        .ToListAsync();

            if (lista is null)
            {
                throw new Exception("Error al obtener la información");
            }

            return lista;
        }

        [HttpGet]
        [Route("ObtenerListaCliente")]
        public async Task<List<Clienteimpresorasfx>> GetClienteimpresorasfxesAsync()
        {
            var lista = await _context.Clienteimpresorasfx
                                        .AsNoTracking()
                                        .ToListAsync();

            if (lista is null)
            {
                throw new Exception("Error al obtener la información");
            }

            return lista;
        }

        [HttpGet]
        [Route("ObtenerListaSolicitante")]
        public async Task<List<Solicitanteimpresorafx>> GetSolicitanteimpresorafxesAsync()
        {
            var lista = await _context.Solicitanteimpresorafx
                                        .AsNoTracking()
                                        .ToListAsync();

            if (lista is null)
            {
                throw new Exception("Error al obtener la información");
            }

            return lista;
        }


        [HttpPost]
        [Route("Registrar")]
        public async Task<IActionResult> Create([FromForm] Impresorasalfx impresora)
        {
            if (impresora == null)
            {
                return BadRequest(new { message = "Los datos de la solicitud son invalidos" });
            }

            try
            {
                if(impresora.FormFile != null && impresora.FormFile.Length > 0)
                {
                    impresora.ArchivoFai = await _azureStorageService.StoreFileFai(_contenedor, impresora.FormFile);
                }
                else
                {
                    impresora.ArchivoFai = null;
                }             

                _context.Impresorasalfx.Add(impresora);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Registro exitoso" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error interno del servidor: {ex.Message}" });
            }
        }

        [HttpPut]
        [Route("Actualizar/{id}")]
        public async Task<IActionResult> Edit(int id, [FromForm] Impresorasalfx impresorasfx)
        {
            try
            {
                var existingEntity = await _context.Impresorasalfx.FindAsync(id);

                if (existingEntity == null)
                {
                    return NotFound(new { message = "Registro no encontrado" });
                }

                foreach (var property in typeof(Impresorasalfx).GetProperties())
                {
                    if (property.Name != nameof(Impresorasalfx.Id) && property.Name != nameof(Impresorasalfx.FormFile))
                    {
                        var value = property.GetValue(impresorasfx);
                        property.SetValue(existingEntity, value);
                    }
                }

                if (impresorasfx.FormFile != null)
                {
                    try
                    {
                        existingEntity.ArchivoFai = await _azureStorageService.StoreFileFai(_contenedor, impresorasfx.FormFile);
                    }
                    catch (Exception ex)
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError, new
                        {
                            mensaje = $"Error al subir el archivo: {ex.Message}"
                        });
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "Registro editado correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    mensaje = $"Error interno del servidor: {ex.Message}",
                    detalle = ex.StackTrace
                });
            }
        }

        [HttpPatch]
        [Route("ActualizarEstatusProyect/{id:int}")]
        public async Task<IActionResult> EditProyecto(int id, [FromBody] Impresorasalfx request)
        {
            var impresoradb = await _context.Impresorasalfx.FindAsync(id);

            if(impresoradb is null)
            {
                return NotFound("Registro no encontrado");
            }

            //impresoradb.EstatusProyectoId = request.EstatusProyectoId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ImpresorasfxExists(id))
                {
                    return NotFound("El registro ya no existe.");
                }
                else
                {
                    throw;
                }
            }

            return Ok(new { message = "Estatus actualizado correctamente", Data = impresoradb });
        }

        private bool ImpresorasfxExists(int id)
        {
            return _context.Impresorasalfx.Any(e => e.Id == id);
        }

        [HttpDelete]
        [Route("Eliminar/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var impresora = await _context.Impresorasalfx.FirstOrDefaultAsync(i => i.Id == id);
            _context.Impresorasalfx.Remove(impresora!);

            await _context.SaveChangesAsync();

            return StatusCode(StatusCodes.Status200OK, new { mensajes = "ok" });
        }
    }
}