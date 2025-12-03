using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using As.Api.Data;
using As.Api.Models;
using System.Threading.Tasks;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;
    public UsersController(AppDbContext db)
    {
        _db = db;
    }

    public record UserCreateDto(string Nome, string Email, string Password);
    public record UserUpdateDto(string Nome, string Email);

    [HttpPost]
    [Authorize] // apenas usuário especial pode criar outros

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var users = await _db.Users
            .Include(u => u.Respostas)
            .Include(u => u.Comments)
            .ToListAsync();

        return Ok(users);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetOne(int id)
    {
        var user = await _db.Users
            .Include(u => u.Respostas)
                .ThenInclude(r => r.Pergunta)
            .Include(u => u.Comments)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return NotFound();

        return Ok(user);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, UserUpdateDto dto)
    {
        var user = await _db.Users.FindAsync(id);

        if (user == null)
            return NotFound();

        user.Nome = dto.Nome;
        user.Email = dto.Email;

        await _db.SaveChangesAsync();
        return Ok(user);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _db.Users.FindAsync(id);

        if (user == null)
            return NotFound();

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
