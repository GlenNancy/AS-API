using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using As.Api.Data;
using As.Api.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System;
using As.Api.Services;

namespace As.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RespostasController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IAccessService _accessService;
        public RespostasController(AppDbContext db, IAccessService accessService)
        {
            _db = db;
            _accessService = accessService;
        }

        // GET: listar todas as respostas (admin)
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var respostas = await _db.Respostas
                .Include(r => r.Pergunta)
                .Include(r => r.User)
                .ToListAsync();

            var result = respostas.Select(r => new
            {
                r.Id,
                r.Texto,
                r.DataCriacao,
                Pergunta = new { r.Pergunta.Id, r.Pergunta.Texto },
                User = new { r.User.Id, r.User.Nome, r.User.Email }
            });

            return Ok(result);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var resposta = await _db.Respostas
                .Include(r => r.Pergunta)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (resposta == null)
                return NotFound("Resposta não encontrada.");

            var result = new
            {
                resposta.Id,
                resposta.Texto,
                resposta.DataCriacao,
                Pergunta = new { resposta.Pergunta.Id, resposta.Pergunta.Texto },
                User = new { resposta.User.Id, resposta.User.Nome, resposta.User.Email }
            };

            return Ok(result);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Resposta model)
        {
            // validação mínima...
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

            // evita duplicidade se quiser: checar se o usuário já respondeu essa pergunta
            var jaRespondeu = await _db.Respostas.AnyAsync(r => r.PerguntaId == model.PerguntaId && r.UserId == userId);
            if (jaRespondeu)
                return BadRequest("Você já respondeu esta pergunta.");

            var resposta = new Resposta
            {
                PerguntaId = model.PerguntaId,
                Texto = model.Texto,
                UserId = userId,
                DataCriacao = DateTime.UtcNow
            };

            _db.Respostas.Add(resposta);
            await _db.SaveChangesAsync();

            // VERIFICAR / GERAR ACESSO
            var recompensa = await _accessService.VerificarOuGerarAcessoAsync(userId);

            return Ok(new
            {
                Mensagem = "Resposta criada com sucesso",
                Id = resposta.Id,
                Recompensa = recompensa // null ou {login, senha, data}
            });
        }


        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Resposta update)
        {
            var resposta = await _db.Respostas.FindAsync(id);
            if (resposta == null)
                return NotFound("Resposta não encontrada.");

            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            //if (resposta.UserId != userId && role != "Admin")
            //    return Forbid("Você não tem permissão para alterar essa resposta.");

            if (string.IsNullOrWhiteSpace(update.Texto))
                return BadRequest("O texto da resposta não pode ser vazio.");

            resposta.Texto = update.Texto;
            await _db.SaveChangesAsync();

            return Ok(new
            {
                resposta.Id,
                resposta.Texto
            });
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var resposta = await _db.Respostas.FindAsync(id);
            if (resposta == null)
                return NotFound("Resposta não encontrada.");

            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            if (resposta.UserId != userId && role != "Admin")
                return Forbid("Você não tem permissão para remover essa resposta.");

            _db.Respostas.Remove(resposta);
            await _db.SaveChangesAsync();

            return Ok("Resposta removida com sucesso.");
        }
    }
}
