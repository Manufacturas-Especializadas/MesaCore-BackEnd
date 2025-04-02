using MesaCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MesaCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImpresionesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ImpresionesController(AppDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        [Route("ObtenerImpresiones")]
        public async Task<IActionResult> GetImpresionesAsync()
        {
            var lista = await _context.Impresionesestadisticas
                                        .Select(i => new
                                        {
                                            i.Id,
                                            FixtureAl = i.FixturesAlNavigation.Codigo,
                                            FixtureCu = i.FixturesCuNavigation.Codigo,
                                            i.Version,
                                            i.Fecha,
                                            i.Impresora,
                                            i.PesoGr,
                                            i.Longitud,
                                            i.TiempoImpresion,
                                            i.PrecioExterno,
                                            i.PrecioInterno,
                                        })
                                        .AsNoTracking()
                                        .ToListAsync();

            if(lista is null)
            {
                throw new Exception("Error al obtener la lista");
            }

            return Ok(lista);
        }

        [HttpGet]
        [Route("ObtenerFixturesAl")]
        public async Task<IActionResult> GetImpresorasfxesAsync()
        {
            var lista = await _context.Impresorasfx
                                        .Select(i => new
                                        {
                                            i.Id,
                                            FixtureAl = i.Codigo
                                        })
                                        .AsNoTracking()
                                        .ToListAsync();

            if(lista is null)
            {
                throw new Exception("Error al obtener la lista");
            }

            return Ok(lista);
        }

        [HttpGet]
        [Route("ObtenerFixturesCu")]
        public async Task<IActionResult> GetImpresorascufxes()
        {
            var lista = await _context.Impresorascufx
                                        .Select(i => new
                                        {
                                            i.Id,
                                            FixtureCu = i.Codigo
                                        })
                                        .AsNoTracking()
                                        .ToListAsync();

            if( lista is null)
            {
                throw new Exception("Errr al obtener la lista");
            }

            return Ok(lista);
        }

        [HttpPost]
        [Route("Registrar")]
        public async Task<IActionResult> Create([FromBody] Impresionesestadisticas impresiones)
        {

            if (impresiones == null)
            {
                return BadRequest("Datos nulos o inválidos");
            }

            if (!impresiones.FixturesAl.HasValue)
            {
                Console.WriteLine("FixturesAl es nulo");
            }

            if (!impresiones.FixturesCu.HasValue)
            {
                Console.WriteLine("FixturesCu es nulo");
            }

            if (!impresiones.Version.HasValue)
            {
                Console.WriteLine("Version es nulo");
            }

            if (string.IsNullOrEmpty(impresiones.Impresora))
            {
                Console.WriteLine("Impresora es nulo o vacío");
            }

            if (!impresiones.PesoGr.HasValue)
            {
                Console.WriteLine("PesoGr es nulo");
            }

            if (!impresiones.Longitud.HasValue)
            {
                Console.WriteLine("Longitud es nulo");
            }

            if (!impresiones.TiempoImpresion.HasValue)
            {
                Console.WriteLine("TiempoImpresion es nulo");
            }

            if (!impresiones.PrecioExterno.HasValue)
            {
                Console.WriteLine("PrecioExterno es nulo");
            }

            if (!impresiones.PrecioInterno.HasValue)
            {
                Console.WriteLine("PrecioInterno es nulo");
            }

            try
            {
                _context.Impresionesestadisticas.Add(impresiones);
                await _context.SaveChangesAsync();
                return Ok(impresiones);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }
    }
}