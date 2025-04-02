using MesaCore.Dtos;
using MesaCore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MesaCore.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<TokenResponseDTO?> LoginAsync(UsuarioDTO request)
        {
            var user = await _context.Usuario.FirstOrDefaultAsync(u => u.Correo == request.Correo);

            if (user == null)
            {
                return null;
            }
            if (new PasswordHasher<Usuario>().VerifyHashedPassword(user, user.Clave, request.Clave) == PasswordVerificationResult.Failed)
            {
                return null;
            }

            return await CreateTokenResponse(user);


        }

        private async Task<TokenResponseDTO> CreateTokenResponse(Usuario? user)
        {
            return new TokenResponseDTO
            {
                AccessToken = CreateToken(user),
                RefreshToken = await GenerateAndSaveRefreshTokenAsync(user)
            };
        }

        public async Task<Usuario?> RegisterAsync(UsuarioDTO request)
        {
            if (await _context.Usuario.AnyAsync(u => u.Correo == request.Correo))
            {
                return null;
            }

            var user = new Usuario();
            var hashedPassword = new PasswordHasher<Usuario>().HashPassword(user, request.Clave);

            user.Nombre = request.Nombre;
            user.Correo = request.Correo;
            user.Clave = request.Clave;

            _context.Usuario.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<TokenResponseDTO?> RefreshTokenAsync(RefreshTokenRequestDTO request)
        {
            var user = await ValidateRefreshTokenAsync(request.Id, request.RefrescarToken);
            if(user == null)
            {
                return null;
            }

            return await CreateTokenResponse(user);
        } 

        private async Task<Usuario?> ValidateRefreshTokenAsync(int id, string refreshToken)
        {
            var user = await _context.Usuario.FindAsync(id);
            if(user == null || user.RefrescarToken != refreshToken || user.FechaDeExpiracionToken <= DateTime.UtcNow)
            {
                return null;
            } 

            return user;
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);

            return Convert.ToBase64String(randomNumber);
        }

        private async Task<string> GenerateAndSaveRefreshTokenAsync(Usuario user)
        {
            var refreshToken = GenerateRefreshToken();
            user.RefrescarToken = refreshToken;
            user.FechaDeExpiracionToken = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            return refreshToken;
        }

        private string CreateToken(Usuario user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Correo),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Rol)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetValue<string>("Jwt:key")!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var tokenDescriptor = new JwtSecurityToken(
                    issuer: _configuration.GetValue<string>("Jwt:Issuer"),
                    audience: _configuration.GetValue<string>("jwt:Audience"),
                    claims: claims,
                    expires: DateTime.UtcNow.AddDays(1),
                    signingCredentials: creds
                );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }
    }
}