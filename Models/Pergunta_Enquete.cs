using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace As.Api.Models
{
    public class Pergunta_Enquete
    {
        public int Id { get; set; }

        [Required]
        public string Texto { get; set; }

        public string TextoProvocativo { get; set; }

        public bool Obrigatoria { get; set; } = false;

        [Required]
        public int EnqueteId { get; set; }
        public Enquete? Enquete { get; set; }

        public ICollection<Resposta> Respostas { get; set; } = new List<Resposta>();
    }
}
