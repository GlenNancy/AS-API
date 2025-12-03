using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace As.Api.Models {
    public class Enquete {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Descricao { get; set; }
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        public bool ContaParaAcesso { get; set; } = false;


        public ICollection<Pergunta_Enquete>? Perguntas { get; set; } 
        public ICollection<Comment>? Comments { get; set; }
    }
}
