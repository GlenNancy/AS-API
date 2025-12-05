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

    private const string RESEND_API_KEY = "re_RGQb2o2K_39yKr5Rc1bzu93oizNuyo7fa";

    public async Task EnviarAcessoAsync(string destino, string login, string senha)
    {
        var mail = new MailMessage()
        {
            From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
            Subject = "Seu acesso foi gerado!",
            IsBodyHtml = true,
            Body = $@"
        <div style='font-family: Arial; padding: 20px; line-height:1.6; color:#111;'>
            
            <h2 style='color:#2563eb;'>Seu acesso foi criado 🎉</h2>

            <p>Obrigado por responder à nossa avaliação.</p>

            <p>
                De forma geral, percebemos que muitos profissionais compartilham desafios como:
            </p>

            <ul style='margin: 15px 0; padding-left: 20px;'>
                <li>Comunicação pouco estratégica ou falta de clareza ao se posicionar.</li>
                <li>Dificuldade de organização e gestão pessoal, reduzindo produtividade.</li>
                <li>Baixa inteligência emocional, afetando foco, decisões e estabilidade.</li>
                <li>Pouca preparação para o futuro — especialmente diante da IA e das novas exigências do mercado.</li>
                <li>Inglês, habilidades técnicas e pensamento crítico abaixo do nível esperado.</li>
            </ul>

            <p>
                É comum que você tenha se identificado com alguns desses pontos — 
                e tudo bem. A diferença está em quem escolhe evoluir antes que os desafios se tornem obstáculos maiores.
            </p>

            <p>
                A boa notícia é que todas essas competências podem ser desenvolvidas. <br>
                E é exatamente para isso que a AS existe.
            </p>

            <hr style='margin:30px 0; border:none; border-top:1px solid #e5e7eb;'>

            <h3 style='color:#2563eb;'>Suas credenciais de acesso</h3>

            <div style='background:#f3f4f6; padding:15px; border-radius:8px;'>
                <p><strong>Login:</strong> {login}</p>
                <p><strong>Senha:</strong> {senha}</p>
            </div>

            <p>Use essas credenciais para entrar na plataforma.</p>

            <p style='margin-top:30px; color:#6b7280; font-size:14px;'>
                AS - Gestão & Performance
            </p>
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
