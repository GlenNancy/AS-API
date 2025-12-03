using Microsoft.AspNetCore.Mvc;
using As.Api.Data;
using As.Api.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;

namespace As.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // validação simples
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password) || string.IsNullOrWhiteSpace(dto.Nome))
                return BadRequest("Nome, email e senha são obrigatórios.");

            if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest("Email already used");

            var user = new User
            {
                Nome = dto.Nome,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, new { user.Id, user.Email, user.Nome });
        }

        // exemplo de endpoint para CreatedAtAction
        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _db.Users
                .Include(u => u.Respostas)
                .ThenInclude(r => r.Pergunta)
                .Include(u => u.Comments)         
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

             if (user == null) return NotFound("Usuário não encontrado.");

            var result = new {
                user.Id,
                user.Nome,
                user.Email,
                user.DataCriacao,
                Respostas = user.Respostas?.Select(r => new {
                    r.Id,
                    r.Texto,
                    Pergunta = new { r.Pergunta.Id, r.Pergunta.Texto },
                    r.DataCriacao
                }),
                Comentarios = user.Comments?.Select(c => new {
                    c.Id,
                    c.Texto,
                    c.EnqueteId,
                    c.DataCriacao
                })
            };

            return Ok(result);
        }

        //[Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _db.Users
                .AsNoTracking()
                .Select(u => new 
                {
                    u.Id,
                    u.Nome,
                    u.Email,
                    Respostas = u.Respostas.Count,
                    Comentarios = u.Comments.Count
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var nomeBusca = dto.Nome?.Trim().ToLower();
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Nome.ToLower() == nomeBusca);

            if (user == null)
                return Unauthorized("Credenciais inválidas");

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Credenciais inválidas");

            var token = GenerateJwtToken(user);
            return Ok(new { token, user = new { user.Id, user.Nome, user.Email, user.Role } });
        }

        private string GenerateJwtToken(User user)
        {
            var keyString = _config["Jwt:Key"];
            var issuer = _config["Jwt:Issuer"];

            if (string.IsNullOrEmpty(keyString))
                throw new InvalidOperationException("Jwt:Key não está configurado. Defina em appsettings.json ou variável de ambiente.");

            var key = Encoding.UTF8.GetBytes(keyString);

            var role = user.Nome?.ToLower() == "admin" ? "Admin" : "User";

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("name", user.Nome ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Role, role)  
            };

            var creds = new SigningCredentials(
                new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: issuer,
                claims: claims,
                expires: DateTime.UtcNow.AddYears(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);

        }
    }

    // DTOs (podem ficar em arquivos separados)
    public record RegisterDto(string Nome, string Email, string Password);
    public record LoginDto(string Nome, string Password);

}
