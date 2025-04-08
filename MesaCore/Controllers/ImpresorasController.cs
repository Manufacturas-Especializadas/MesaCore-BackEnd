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
                var query = _context.Impresorasfx.AsQueryable();

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
                                   i.Codigo,
                                   PlantId = i.Planta.Nombre,
                                   SolicitanteId = i.Solicitante.Nombre,
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
            var query = await _context.Impresorasfx
                .Select(i => new
                {
                    i.Id,
                    i.NombreDelProyecto,
                    i.FechaDeLaSolicitud,
                    SolicitanteNombre = i.Solicitante.Nombre,
                    i.NParte,
                    EstatusNombre = i.Estatus.Nombre,
                    EstatusProyecto = i.EstatusProyecto.Nombre
                })
                .ToListAsync();

            var lista = query
                .GroupBy(i => i.NombreDelProyecto)
                .Select(g => g.OrderBy(x => x.SolicitanteNombre).First())
                .OrderBy(i => i.NombreDelProyecto)
                .ThenBy(i => i.SolicitanteNombre)
                .ToList();

            if (lista == null)
            {
                throw new Exception("Error al obtener los datos");
            }

            return Ok(lista);
        }

        [HttpGet]
        [Route("ObtenerImpresoraPorId/{id}")]
        public async Task<IActionResult> GetImpresoraPorIdAsync(int id)
        {
            var impresorId = await _context.Impresorasfx.FirstOrDefaultAsync(i => i.Id == id);

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
            var impresoras = await _context.Impresorasfx
                                        .Where(i => i.NombreDelProyecto == nombreDelProyecto)
                                        .Select(i => new
                                        {
                                            i.Id,
                                            i.NombreDelProyecto,
                                            i.Estatus,
                                            i.Planta,
                                            i.Solicitante,
                                            i.Cliente,
                                            i.NParte,
                                            i.NDibujo,
                                            i.Revision,
                                            i.EstatusProyecto
                                        })
                                        .ToListAsync();

            if (impresoras == null || impresoras.Count == 0)
            {
                return NotFound("No se encontraron impresoras con este nombre de proyecto.");
            }

            var result = impresoras.GroupBy(i => i.NombreDelProyecto).Select(g => new
            {
                ID = g.First().Id,
                EstatusProyecto = g.First().EstatusProyecto.Nombre,
                nombreDelProyecto = g.Key,
                Estatus = g.First().Estatus.Nombre,
                Planta = g.First().Planta.Nombre,
                Solicitante = g.First().Solicitante.Nombre,
                Cliente = g.First().Cliente.Nombre,
                impresiones = g.Select(i => new
                {
                    i.Id,
                    i.NParte,
                    i.NDibujo,
                    i.Revision,
                    i.Estatus,
                    i.Planta,
                    i.Cliente,
                    i.Solicitante,
                    i.EstatusProyecto
                }).ToList()
            }).FirstOrDefault();

            return Ok(result);
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
        public async Task<Impresorasfx> Create([FromForm] Impresorasfx impresora)
        {
            if (impresora is null || impresora.FormFile == null || impresora.FormFile.Length == 0)
            {
                throw new Exception("Error al registrar la información, debe ingresar un archivo");
            }

            try
            {
                //impresora.ArchivoFai = await _azureStorageService.StoreFileFai(_contenedor, impresora.FormFile);

                await _context.AddAsync(impresora);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al registrar la información: {ex.Message}");
            }

            await _context.Impresorasfx.AddAsync(impresora);
            await _context.SaveChangesAsync();

            return impresora;
        }

        [HttpPut]
        [Route("Actualizar")]
        public async Task<IActionResult> Edit([FromBody] Impresorasfx impresorasfx)
        {
            _context.Impresorasfx.Update(impresorasfx);
            await _context.SaveChangesAsync();

            return StatusCode(StatusCodes.Status200OK, new { mensaje = "ok" });
        }

        [HttpPatch]
        [Route("ActualizarEstatusProyect/{id:int}")]
        public async Task<IActionResult> EditProyecto(int id, [FromBody] Impresorasfx request)
        {
            var impresoradb = await _context.Impresorasfx.FindAsync(id);

            if(impresoradb is null)
            {
                return NotFound("Registro no encontrado");
            }

            impresoradb.EstatusProyectoId = request.EstatusProyectoId;

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
            return _context.Impresorasfx.Any(e => e.Id == id);
        }

        [HttpDelete]
        [Route("Eliminar/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var impresora = await _context.Impresorasfx.FirstOrDefaultAsync(i => i.Id == id);
            _context.Impresorasfx.Remove(impresora!);

            await _context.SaveChangesAsync();

            return StatusCode(StatusCodes.Status200OK, new { mensajes = "ok" });
        }
    }
}