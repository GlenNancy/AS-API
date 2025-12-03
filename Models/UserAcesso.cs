namespace As.Api.Models
{
    public class UserAcesso
    {
        public int Id { get; set; }

        // FK para o usuário (quem recebeu o acesso)
        public int UserId { get; set; }
        public User User { get; set; }

        public string LoginGerado { get; set; }

        // Armazenar apenas hash por segurança
        public string SenhaHash { get; set; }

        public DateTime DataGeracao { get; set; } = DateTime.UtcNow;
    }
}
