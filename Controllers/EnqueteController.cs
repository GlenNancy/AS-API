using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using As.Api.Data;
using As.Api.Models;
using As.Api.Dtos;
using Microsoft.AspNetCore.Authorization;

namespace As.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnqueteController : ControllerBase
    {
        private readonly AppDbContext _db;

        public EnqueteController(AppDbContext db)
        {
            _db = db;
        }

        // -----------------------------------------
        // GET ALL - retorna EnqueteDto (leve)
        // -----------------------------------------
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var enquetes = await _db.Enquetes
                            .Include(e => e.Perguntas) // traz as perguntas
                            .ToListAsync();

            var dtoList = await _db.Enquetes
        .Select(e => new EnqueteDto
        {
            Id = e.Id,
            Titulo = e.Titulo,
            Descricao = e.Descricao, 
            DataCriacao = e.DataCriacao,
            ContaParaAcesso = e.ContaParaAcesso,
            Perguntas = e.Perguntas
                .Select(pe => new PerguntaDto
                {
                    Id = pe.Id,
                    Texto = pe.Texto,
                    TextoProvocativo = pe.TextoProvocativo
                }).ToList()
        }).ToListAsync();


            return Ok(dtoList);
        }

        // -----------------------------------------
        // GET POR ID - retorna EnqueteDetalhadaDto
        // -----------------------------------------
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var enquete = await _db.Enquetes
                .Include(e => e.Perguntas)
                    .ThenInclude(p => p.Respostas)
                    .ThenInclude(r => r.User)
                .Include(e => e.Comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (enquete == null)
                return NotFound("Enquete não encontrada.");

            enquete.Perguntas = enquete.Perguntas
                .Where(p => p != null)
                .ToList();

            var dto = new EnqueteDetalhadaDto
            {
                Id = enquete.Id,
                Titulo = enquete.Titulo,
                Descricao = enquete.Descricao,
                DataCriacao = enquete.DataCriacao,
                ContaParaAcesso = enquete.ContaParaAcesso,

                Perguntas = enquete.Perguntas?.Select(p => new PerguntaDto
                {
                    Id = p.Id,
                    Texto = p.Texto,
                    Obrigatoria = p.Obrigatoria,

                    Respostas = p.Respostas?.Select(r => new RespostaDto
                    {
                        Id = r.Id,
                        Texto = r.Texto,
                        DataCriacao = r.DataCriacao,

                        User = r.User == null ? null : new UserDto
                        {
                            Id = r.User.Id,
                            Nome = r.User.Nome,
                            Email = r.User.Email
                        }

                    }).ToList()

                }).ToList(),

                Comments = enquete.Comments?.Select(c => new As.Api.Dtos.CommentDto
                {
                    Id = c.Id,
                    Texto = c.Texto,
                    DataCriacao = c.DataCriacao,

                    User = c.User == null ? null : new As.Api.Dtos.UserDto
                    {
                        Id = c.User.Id,
                        Nome = c.User.Nome,
                        Email = c.User.Email
                    }

                }).ToList()
            };

            return Ok(dto);
        }


        // -----------------------------------------
        // POST
        // -----------------------------------------
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(Enquete model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _db.Enquetes.Add(model);
            await _db.SaveChangesAsync();

            var dto = new EnqueteDto
            {
                Id = model.Id,
                Titulo = model.Titulo,
                Descricao = model.Descricao,
                DataCriacao = model.DataCriacao
            };

            return Ok(dto);
        }

        // -----------------------------------------
        // PUT
        // -----------------------------------------
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Enquete update)
        {
            var enquete = await _db.Enquetes.FindAsync(id);
            if (enquete == null)
                return NotFound("Enquete não encontrada.");

            enquete.Titulo = update.Titulo;
            enquete.Descricao = update.Descricao;

            await _db.SaveChangesAsync();

            return Ok(new EnqueteDto
            {
                Id = enquete.Id,
                Titulo = enquete.Titulo,
                Descricao = enquete.Descricao,
                DataCriacao = enquete.DataCriacao
            });
        }

        // -----------------------------------------
        // DELETE
        // -----------------------------------------
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var enquete = await _db.Enquetes.FindAsync(id);

            if (enquete == null)
                return NotFound("Enquete não encontrada.");

            _db.Enquetes.Remove(enquete);
            await _db.SaveChangesAsync();

            return Ok("Enquete removida com sucesso.");
        }
    }
}
