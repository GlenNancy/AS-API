using As.Api.Data;
using As.Api.Models;
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

        public UserAcessosController(AppDbContext db)
        {
            _db = db;
        }

        public class GerarAcessoRequest
        {
            public int UserId { get; set; }
        }


        // ============================================================
        // 🟢 1. GERAR ACESSO (SEM ENVIAR E-MAIL, SÓ RETORNANDO NA TELA)
        // ============================================================
        [HttpPost("gerar")]
        public async Task<IActionResult> GerarAcesso([FromBody] GerarAcessoRequest req)
        {
            var user = await _db.Users.FindAsync(req.UserId);
            if (user == null)
                return NotFound(new { mensagem = "Usuário não encontrado." });

            // Verificar perguntas obrigatórias (igual estava)
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

            // Verifica se já existe acesso
            /*var acessoExistente = await _db.UserAcessos
                .FirstOrDefaultAsync(a => a.UserId == req.UserId);

            if (acessoExistente != null)
            {
                return Ok(new
                {
                    mensagem = "Seu acesso já havia sido gerado anteriormente. Use o login abaixo. A senha não pode ser exibida novamente.",
                    login = acessoExistente.LoginGerado
                });
            }*/

            // 🔹 Define o login a partir do próprio usuário (ajuste o nome da propriedade de e-mail se for diferente)
            var login = !string.IsNullOrWhiteSpace(user.Email)
                ? user.Email.Trim().ToLowerInvariant()
                : $"user{req.UserId}";

            // Gera senha aleatória e salva hash
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

            // ✅ NÃO envia e-mail, apenas retorna
            return Ok(new
            {
                mensagem = "Acesso gerado com sucesso. Guarde esse login e senha, eles não serão enviados por e-mail.",
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
