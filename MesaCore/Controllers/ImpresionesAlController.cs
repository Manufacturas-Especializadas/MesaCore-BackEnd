using MesaCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MesaCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImpresionesAlController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ImpresionesAlController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("ObtenerPorId")]
        public async Task<IActionResult> GetCodeById(int id)
        {
            var impressionsById = await _context.Impresionesalfx.FirstOrDefaultAsync(i => i.Id == id);

            if(impressionsById == null)
            {
                return NotFound(StatusCode(StatusCodes.Status404NotFound, new { message = "Id no encontrado" }));
            }

            return Ok(StatusCode(StatusCodes.Status200OK, impressionsById));
        }

        [HttpGet]
        [Route("Paginacion")]
        public async Task<IActionResult> GetPaginacion(
                                                    [FromQuery] int page = 1,
                                                    [FromQuery] int pageSize = 10,
                                                    [FromQuery] string codigo = null!                                                    
            )
        {
            try
            {
                var query = _context.Impresionesalfx.AsQueryable();

                if (!string.IsNullOrEmpty(codigo))
                {
                    query = query.Where(p => p.Codigo.Contains(codigo));
                }

                var uniqueCodes = query
                                    .GroupBy(p => p.Codigo)
                                    .Select(g => g.Min(i => i.Id))
                                    .ToList();

                var impressionsQuery = _context.Impresionesalfx
                                                .Where(p => uniqueCodes.Contains(p.Id))
                                                .OrderBy(p => p.Codigo);

                var totalRecords = impressionsQuery.Count();

                var impressions = await impressionsQuery
                                            .Skip((page - 1) * pageSize)
                                            .Take(pageSize)
                                            .Select(i => new
                                            {
                                                i.Id,
                                                i.Codigo,
                                                i.Version,
                                                i.Fecha,
                                                i.PesoGr,
                                                i.Longitud,
                                                i.TiempoImpresion,
                                                i.PrecioInterno,
                                                i.PrecioExterno
                                            })
                                            .AsNoTracking()
                                            .ToListAsync(); 


                return Ok(new { data = impressions, totalRecords, page, pageSize });
            }
            catch ( Exception ex )
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error interno: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("DetallesPorCodigo/{codigo}")]
        public async Task<IActionResult> GetDetailsByCode(string codigo)
        {
            try
            {
                if (string.IsNullOrEmpty(codigo))
                {
                    return BadRequest("El codigo no puede estar vacio");
                }

                var details = await _context.Impresionesalfx
                                                .Where(p => p.Codigo == codigo)
                                                .OrderByDescending(p => p.Fecha)
                                                .Select(i => new
                                                {
                                                    i.Id,
                                                    i.Codigo,
                                                    i.Version,
                                                    i.Fecha,
                                                    i.PesoGr,
                                                    i.Longitud,
                                                    i.TiempoImpresion,
                                                    i.PrecioExterno,
                                                    i.PrecioInterno
                                                })
                                                .AsNoTracking()
                                                .ToListAsync();

                if(details == null || details.Count == 0)
                {
                    return NotFound($"No se encontrarón registros para el código {codigo}");
                }

                return Ok(details);
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error interno: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("RegistrarNuevaImpresion")]
        public async Task<IActionResult> Create([FromForm] Impresionesalfx impresionesalfx)
        {
            if(impresionesalfx == null)
            {
                return BadRequest(StatusCode(StatusCodes.Status400BadRequest, new { message = "Hubo un error al registrar" }));
            }

            _context.Impresionesalfx.Add(impresionesalfx);
            await _context.SaveChangesAsync();

            return Ok(StatusCode(StatusCodes.Status200OK, new { message = "Se registro exitosamente" }));
        }

        [HttpPatch]
        [Route("EditarRegistro/{id:int}")]
        public async Task<IActionResult> Edit([FromForm] Impresionesalfx impresionesalfx ,int id)
        {
            var impression = await _context.Impresionesalfx.FindAsync(id);

            if(impression == null)
            {
                return BadRequest(StatusCode(StatusCodes.Status400BadRequest, new { message = "Hubo un error al editar" }));
            }

            _context.Impresionesalfx.Update(impression);
            await _context.SaveChangesAsync();

            return Ok(StatusCode(StatusCodes.Status200OK, new { message = "Se editó correctamente" }));

        }

        [HttpDelete]
        [Route("EliminarRegistro/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var impresora = await _context.Impresionesalfx.FirstOrDefaultAsync(i => i.Id == id);

            if (impresora == null)
            {
                return NotFound(StatusCode(StatusCodes.Status404NotFound, new { message = "Registro no encontrado" }));
            }

            _context.Impresionesalfx.Remove(impresora);
            await _context.SaveChangesAsync();


            return StatusCode(StatusCodes.Status200OK, new { message = "Registro eliminado" });
        }
    }
}