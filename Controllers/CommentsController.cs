using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using As.Api.Data;
using As.Api.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;

[ApiController]
[Route("api/[controller]")]
public class CommentsController : ControllerBase
{
    private readonly AppDbContext _db;
    public CommentsController(AppDbContext db) { _db = db; }

    public record CommentCreateDto(int EnqueteId, string Texto);
    public record CommentUpdateDto(string Texto);

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(CommentCreateDto dto)
    {
        var enquete = await _db.Enquetes.FindAsync(dto.EnqueteId);
        if (enquete == null)
            return NotFound("Enquete não encontrada.");

        var userId = int.Parse(
            User.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
            User.FindFirstValue(ClaimTypes.NameIdentifier)
        );

        var comment = new Comment
        {
            EnqueteId = dto.EnqueteId,
            UserId = userId,
            Texto = dto.Texto
        };

        _db.Comments.Add(comment);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            comment.Id,
            comment.EnqueteId,
            comment.UserId,
            comment.Texto,
            comment.DataCriacao
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var comments = await _db.Comments
            .Include(c => c.User)
            .ToListAsync();

        return Ok(comments);
    }

    [HttpGet("enquete/{enqueteId}")]
    public async Task<IActionResult> GetByEnquete(int enqueteId)
    {
        var comments = await _db.Comments
            .Where(c => c.EnqueteId == enqueteId)
            .Include(c => c.User)
            .ToListAsync();

        return Ok(comments);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, CommentUpdateDto dto)
    {
        var comment = await _db.Comments.FindAsync(id);
        if (comment == null) return NotFound();

        var userId = int.Parse(
            User.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
            User.FindFirstValue(ClaimTypes.NameIdentifier)
        );

        if (comment.UserId != userId)
            return Forbid();

        comment.Texto = dto.Texto;
        await _db.SaveChangesAsync();

        return Ok(comment);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var comment = await _db.Comments.FindAsync(id);
        if (comment == null) return NotFound();

        var userId = int.Parse(
            User.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
            User.FindFirstValue(ClaimTypes.NameIdentifier)
        );

        if (comment.UserId != userId)
            return Forbid();

        _db.Comments.Remove(comment);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
