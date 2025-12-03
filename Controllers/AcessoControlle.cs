using As.Api.Data;
using As.Api.Models;
using As.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace As.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserAcessosController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IEmailService _emailService;

        public UserAcessosController(AppDbContext db, IEmailService emailService)
        {
            _db = db;
            _emailService = emailService;
        }

        public class GerarAcessoRequest
        {
            public int UserId { get; set; }
            public string Email { get; set; }
        }

        // ============================================================
        // 🟢 1. GERAR ACESSO + ENVIAR EMAIL
        // ============================================================
        [HttpPost("gerar")]
        public async Task<IActionResult> GerarAcesso([FromBody] GerarAcessoRequest req)
        {
            var user = await _db.Users.FindAsync(req.UserId);
            if (user == null)
                return NotFound("Usuário não encontrado.");

            if (string.IsNullOrWhiteSpace(req.Email))
                return BadRequest("O campo 'email' é obrigatório.");

            var obrigatorias = await _db.Perguntas
                .Where(p => p.Obrigatoria)
                .Include(p => p.Respostas)
                .ToListAsync();

            var faltando = obrigatorias
                .Where(p => !p.Respostas.Any(r => r.UserId == req.UserId))
                .ToList();

            if (faltando.Count > 0)
            {
                return BadRequest(new
                {
                    mensagem = "Você ainda não respondeu todas as perguntas obrigatórias.",
                    faltando = faltando.Select(f => new { f.Id, f.Texto })
                });
            }

            var acessoExistente = await _db.UserAcessos
                .FirstOrDefaultAsync(a => a.UserId == req.UserId);

            if (acessoExistente != null)
            {
                return Ok(new
                {
                    mensagem = "Acesso já existe.",
                    login = acessoExistente.LoginGerado
                });
            }

            string login = $"user{req.UserId}";
            string senha = Guid.NewGuid().ToString("N")[..8];
            string hash = HashSenha(senha);

            var novo = new UserAcesso
            {
                UserId = req.UserId,
                LoginGerado = login,
                SenhaHash = hash,
                DataGeracao = DateTime.UtcNow
            };

            _db.UserAcessos.Add(novo);
            await _db.SaveChangesAsync();

            // 🔥 Enviar email para o usuário
            await _emailService.EnviarAcessoAsync(req.Email, login, senha);

            return Ok(new
            {
                mensagem = "Acesso gerado e enviado por e-mail.",
                login,
                senha
            });
        }

        // ============================================================
        // 🟡 2. CONSULTAR ACESSO DO USUÁRIO
        // ============================================================
        [HttpGet("consultar/{userId}")]
        public async Task<IActionResult> Consultar(int userId)
        {
            var acesso = await _db.UserAcessos
                .Include(u => u.User)
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (acesso == null)
                return Ok(new { possuiAcesso = false });

            return Ok(new
            {
                possuiAcesso = true,
                login = acesso.LoginGerado,
                criadoEm = acesso.DataGeracao
            });
        }

        // ============================================================
        // 🟣 3. LISTAR TODOS
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> ListarTodos()
        {
            var acessos = await _db.UserAcessos
                .Include(a => a.User)
                .ToListAsync();

            return Ok(acessos);
        }

        // ============================================================
        // ❌ 4. REMOVER
        // ============================================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Deletar(int id)
        {
            var acesso = await _db.UserAcessos.FindAsync(id);
            if (acesso == null)
                return NotFound("Acesso não encontrado.");

            _db.UserAcessos.Remove(acesso);
            await _db.SaveChangesAsync();

            return Ok("Acesso removido.");
        }

        private string HashSenha(string senha)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(senha);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }
    }
}
