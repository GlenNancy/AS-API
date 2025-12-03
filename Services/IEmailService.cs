namespace As.Api.Services
{
    public interface IEmailService
    {
        Task EnviarAcessoAsync(string destinoEmail, string login, string senha);
    }
}
