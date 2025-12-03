using System;

namespace As.Api.Models {
    public class Resposta {
        public int Id { get; set; }
        public int PerguntaId { get; set; }
        public int UserId { get; set; }
        public string Texto { get; set; }
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        public Pergunta_Enquete? Pergunta { get; set; }
        public User? User { get; set; }
    }
}
