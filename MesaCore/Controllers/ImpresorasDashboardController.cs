using MesaCore.Models;
using MesaCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MesaCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImpresorasDashboardController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AzureStorageService _azureStorageService;
        private readonly EmailServices _emailServices;
        private readonly string _contenedor = "fileprinters";

        public ImpresorasDashboardController(AppDbContext context, AzureStorageService azureStorageService, 
                EmailServices emailServices)
        {
            _context = context;
            _azureStorageService = azureStorageService;
            _emailServices = emailServices;
        }

        [HttpPost]
        [Route("Registro")]
        public async Task<IActionResult> Create([FromForm] Registrodeimpresorasfx registro)
        {
            if (registro == null || registro.FormFile == null || registro.FormFile.Length == 0)
            {
                return BadRequest("Debe proporcionar un archivo válido.");
            }

            try
            {                
                registro.Archivo = await _azureStorageService.StoreFiles(_contenedor, registro.FormFile);
              
                _context.Add(registro);
                await _context.SaveChangesAsync();

                var registroConSolicitante = await _context.Registrodeimpresorasfx
                                                        .Include(r => r.Solicitante)
                                                        .FirstOrDefaultAsync(r => r.Id == registro.Id);

                var emailBody = $@"
                        <h1>Proyecto: {registro.NombreDelProyecto ?? "Sin nombre"}</h1>
                        <p><strong>Solicitante: </strong> {registro.Solicitante?.Nombre ?? "Sin solicitante"}</p>
                        <p><strong>Fecha de solicitud: </strong> {registro.FechaDeSolicitud?.ToString("dd/MM/yyyy") ?? "Sin fecha"}</p>
                        <p><strong>Comentarios: </strong> {registro.Comentarios ?? "Sin comentarios"}</p>
                        <p><strong>Archivo: </strong> {(string.IsNullOrEmpty(registro.Archivo) ? "Sin archivo" : $"<a href='{registro.Archivo}'>Descargar archivo</a>")}</p>";

                await _emailServices.SendEmailAsync("angel.medina@mesa.ms", "Nuevo registro", emailBody);

                return Ok(new
                {
                    registro.Id,
                    registro.NombreDelProyecto,
                    registro.FechaDeSolicitud,
                    registro.Comentarios,
                    registro.Archivo,
                    Solicitante = new { registro.Solicitante?.Id, registro.Solicitante?.Nombre }
                });
            }
            catch (Exception ex)
            {
              return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

    }
}