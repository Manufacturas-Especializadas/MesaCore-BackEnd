using MesaCore.Dtos;
using MesaCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MesaCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProyectosCUController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProyectosCUController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("ObtenerPorId")]
        public async Task<IActionResult> GetProjectById(int id)
        {
            var projectId = await _context.Proyectosfxcu.FirstOrDefaultAsync(p => p.Id == id);

            if (projectId == null)
            {
                return NotFound(StatusCode(StatusCodes.Status404NotFound, new { message = "Id no encontrado" }));
            }

            return Ok(StatusCode(StatusCodes.Status200OK, projectId));
        }

        [HttpGet]
        [Route("ObtenerListaDeProyectos")]
        public async Task<IActionResult> GetProjects()
        {
            var lista = await _context.Proyectosfxcu
                                    .Select(p => new
                                    {
                                        p.Id,
                                        p.NombreDelProyecto,
                                        p.FechaDeLaSolicitud,
                                        Estatus = p.Estatus.Nombre,
                                        Planta = p.Planta.Nombre,
                                        Solicitante = p.Solicitante.Nombre
                                    })
                                    .AsNoTracking()
                                    .ToListAsync();

            if (lista is null)
            {
                throw new Exception("Sin datos");
            }

            return Ok(lista);
        }

        [HttpGet]
        [Route("ObtenerSolicitanteProyectos")]
        public async Task<List<Solicitanteimpresorafx>> GetSolicitanteimpresorafxesAsync()
        {
            var lista = await _context.Solicitanteimpresorafx
                                        .AsNoTracking()
                                        .ToListAsync();

            if (lista is null)
            {
                throw new Exception("No hay datos disponibles");
            }


            return lista;
        }

        [HttpGet]
        [Route("ObtenerPlantaProyectos")]
        public async Task<List<Plantaimpresorasfx>> GetPlantaimpresorasfxesAsync()
        {
            var lista = await _context.Plantaimpresorasfx
                                        .AsNoTracking()
                                        .ToListAsync();
            if (lista is null)
            {
                throw new Exception("No hay datos disponibles");
            }

            return lista;
        }

        [HttpGet]
        [Route("ObtenerEstatusProyectos")]
        public async Task<List<Estatusproyectoimpresorasfx>> GetEstatusproyectoimpresorasfxesAsync()
        {
            var lista = await _context.Estatusproyectoimpresorasfx
                                        .AsNoTracking()
                                        .ToListAsync();

            if (lista is null)
            {
                throw new Exception("No hay datos disponibles");
            }

            return lista;
        }

        [HttpPost]
        [Route("Registrar")]
        public async Task<IActionResult> Create([FromBody] Proyectosfxcu proyectos)
        {
            if (proyectos is null)
            {
                return BadRequest("No puedes enviar valores nulos");
            }

            await _context.Proyectosfxcu.AddAsync(proyectos);
            await _context.SaveChangesAsync();

            return Ok(StatusCode(StatusCodes.Status200OK, new { message = "Registro exitoso" }));
        }

        [HttpPut]
        [Route("Editar/{id:int}")]
        public async Task<IActionResult> Edit([FromBody] ProyectoUpdateCUDto dto, int id)
        {
            var proyecto = await _context.Proyectosfxcu.FindAsync(id);
            if (proyecto == null) return BadRequest("No encontrado");

            proyecto.NombreDelProyecto = dto.NombreDelProyecto;
            proyecto.SolicitanteId = dto.SolicitanteId;
            proyecto.PlantaId = dto.PlantaId;
            proyecto.EstatusId = dto.EstatusId;

            proyecto.FechaDeLaSolicitud = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Editado correctamente" });
        }

        [HttpDelete]
        [Route("Eliminar")]
        public async Task<IActionResult> Delete(int id)
        {
            var registroId = await _context.Proyectosfxcu.FirstOrDefaultAsync(x => x.Id == id);

            if (registroId == null) return BadRequest("No se encontró el id seleccionado");

            _context.Proyectosfxcu.Remove(registroId);
            await _context.SaveChangesAsync();

            return StatusCode(StatusCodes.Status200OK, new { message = "Registro eliminado" });
        }
    }
}