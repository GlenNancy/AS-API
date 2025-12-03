using System.Net;
using System.Net.Mail;
using As.Api.Services;
using As.Api.Settings;
using Microsoft.Extensions.Options;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task EnviarAcessoAsync(string destino, string login, string senha)
    {
        var mail = new MailMessage()
        {
            From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
            Subject = "Seu acesso foi gerado!",
            IsBodyHtml = true,
            Body = $@"
                <div style='font-family: Arial; padding: 20px;'>
                    <h2 style='color:#2563eb;'>Seu acesso foi criado 🎉</h2>
                    <p>Aqui estão suas credenciais:</p>

                    <div style='background:#f3f4f6; padding:15px; border-radius:8px;'>
                        <p><strong>Login:</strong> {login}</p>
                        <p><strong>Senha:</strong> {senha}</p>
                    </div>

                    <p>Use essas credenciais para entrar na plataforma.</p>

                    <p style='margin-top:20px; color:#6b7280;'>AS - Gestão & Performance</p>
                </div>"
        };

        mail.To.Add(destino);

        using var smtp = new SmtpClient("smtp.gmail.com", 587)
        {
            Credentials = new NetworkCredential(_settings.SenderEmail, _settings.Password), // SENHA DE APP!
            EnableSsl = true
        };

        await smtp.SendMailAsync(mail);
    }
}
