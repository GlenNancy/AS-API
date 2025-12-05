using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using As.Api.Data;
using As.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using As.Api.Dtos;

[ApiController]
[Route("api/[controller]")]
public class PerguntasController : ControllerBase
{
    private readonly AppDbContext _db;
    public PerguntasController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(Pergunta_Enquete model)
    {
        var enquete = await _db.Enquetes.FindAsync(model.EnqueteId);
        if (enquete == null)
            return NotFound("Enquete não encontrada.");

        _db.Perguntas.Add(model);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetOne), new { id = model.Id }, model);
    }

    [HttpGet("enquete/{enqueteId}")]
    public async Task<IActionResult> GetByEnquete(int enqueteId)
    {
        var perguntas = await _db.Perguntas
            .Where(p => p.EnqueteId == enqueteId)
            .Include(p => p.Respostas)
            .ToListAsync();

        return Ok(perguntas);
    }

    [HttpGet("perguntas-obrigatorias/{userId}")]
    public async Task<IActionResult> VerificarObrigatorias(int userId)
    {
        // pega todas perguntas obrigatórias
        var perguntasObrigatorias = await _db.Perguntas
            .Where(p => p.Obrigatoria == true)
            .Include(p => p.Respostas)
            .ToListAsync();

        if (perguntasObrigatorias.Count == 0)
        {
            return Ok(new
            {
                respondeuTudo = true,
                faltando = Array.Empty<object>()
            });
        }

        // filtra as perguntas que o user AINDA NÃO respondeu
        var faltando = perguntasObrigatorias
            .Where(p => !p.Respostas.Any(r => r.UserId == userId))
            .Select(p => new
            {
                p.Id,
                p.Texto
            })
            .ToList();

        bool respondeuTudo = faltando.Count == 0;

        return Ok(new
        {
            respondeuTudo,
            faltando
        });
    }


    [HttpGet("obrigatorias")]
    public async Task<IActionResult> GetAllObrigatorias()
    {
        var perguntas = await _db.Perguntas
            .Where(p => p.Obrigatoria == true)
            .Include(p => p.Enquete)
            .Include(p => p.Respostas)
            .ToListAsync();

        return Ok(perguntas);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOne(int id)
    {
        var pergunta = await _db.Perguntas
            .Include(p => p.Enquete)
            .Include(p => p.Respostas)
            .FirstOrDefaultAsync(p => p.Id == id);


        if (pergunta == null)
            return NotFound();

        return Ok(pergunta);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var perguntas = await _db.Perguntas
            .Include(p => p.Respostas)
            .ThenInclude(r => r.User)
            .ToListAsync();

        var dtoList = perguntas.Select(p => new PerguntaDto
        {
            Id = p.Id,
            Texto = p.Texto,
            Obrigatoria = p.Obrigatoria,

            Respostas = p.Respostas.Select(r => new RespostaDto
            {
                Id = r.Id,
                Texto = r.Texto,
                DataCriacao = r.DataCriacao,

                User = r.User == null
                    ? null
                    : new UserDto
                    {
                        Id = r.User.Id,
                        Nome = r.User.Nome,
                        Email = r.User.Email
                    }
            }).ToList()
        }).ToList();

        return Ok(dtoList);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, Pergunta_Enquete model)
    {
        var pergunta = await _db.Perguntas.FindAsync(id);
    
        if (pergunta == null)
            return NotFound("Pergunta não encontrada.");
    
        // Se o usuário enviar um EnqueteId, valida se ela existe
        if (model.EnqueteId != pergunta.EnqueteId)
        {
            var enquete = await _db.Enquetes.FindAsync(model.EnqueteId);
            if (enquete == null)
                return NotFound("A Enquete enviada não existe.");
        }
    
        // Atualiza os campos (inclusive o novo TextoProvocativo)
        pergunta.Texto = model.Texto;
        pergunta.TextoProvocativo = model.TextoProvocativo;
        pergunta.Obrigatoria = model.Obrigatoria;
        pergunta.EnqueteId = model.EnqueteId;
    
        await _db.SaveChangesAsync();
    
        return Ok(new { message = "Pergunta atualizada com sucesso!", pergunta });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var pergunta = await _db.Perguntas.FindAsync(id);

        if (pergunta == null)
            return NotFound();

        _db.Perguntas.Remove(pergunta);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Pergunta deletada com sucesso!" });
    }
}
