using Microsoft.AspNetCore.Mvc;
using As.Api.Services;

[ApiController]
[Route("api/test-email")]
public class TestEmailController : ControllerBase
{
    private readonly IEmailService _email;

    public TestEmailController(IEmailService email)
    {
        _email = email;
    }

    [HttpGet]
    public async Task<IActionResult> Send()
    {
        await _email.EnviarAcessoAsync("seuemail@gmail.com", "login_teste", "senha_teste");
        return Ok("Enviado");
    }
}
