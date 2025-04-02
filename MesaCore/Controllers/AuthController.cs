using MesaCore.Models;
using MesaCore.Dtos;
using MesaCore.Utilidades;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace MesaCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly Encriptar _encriptar;

        public AuthController(AppDbContext context, Encriptar encriptar)
        {
            _context = context;
            _encriptar = encriptar;
        }

        [HttpPost]
        [Route("Registrarse")]
        public async Task<IActionResult> Registro(UsuarioDTO usuarioDTO)
        {
            var usuarioExistente = await _context.Usuario.AnyAsync(u => u.Correo == usuarioDTO.Correo);

            if (usuarioExistente)
            {
                return BadRequest(new { isSuccess = false, message = "El correo ya esta registrado" });
            }

            var modelo = new Usuario
            {
                Nombre = usuarioDTO.Nombre,
                Correo = usuarioDTO.Correo,
                Clave = _encriptar.HashPassword(usuarioDTO.Clave),
                Rol = usuarioDTO.Rol,
            };

            await _context.AddAsync(modelo);
            await _context.SaveChangesAsync();

            return Ok(new { isSuccess = modelo.Id != 0 });
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Correo == loginDTO.Correo);

            if (usuario == null || !_encriptar.VerifyPassword(usuario.Clave, loginDTO.Clave))
            {
                return Ok(new { isSuccess = false, token = "" });
            }

            string token = _encriptar.generarJWT(usuario);
            string refreshToken = _encriptar.RefreshToken();

            usuario.RefrescarToken = refreshToken;
            usuario.Rol = loginDTO.Rol;
            usuario.FechaDeExpiracionToken = DateTime.UtcNow.AddDays(7);

            await _context.SaveChangesAsync();

            return Ok(new { isSuccess = true, token });
        }

        [HttpPost]
        [Route("RefrescarToken")]
        public async Task<IActionResult> RefrescarToken([FromBody] string refreshToken)
        {
            var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.RefrescarToken == refreshToken);

            if (usuario == null || usuario.FechaDeExpiracionToken < DateTime.UtcNow)
            {
                return Unauthorized(new { message = "Token invalido o expirado" });
            }

            string newToken = _encriptar.generarJWT(usuario);

            return Ok(new { token = newToken });
        }
    }
}