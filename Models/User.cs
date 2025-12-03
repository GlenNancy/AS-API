using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace As.Api.Models {
    public class User {
        public int Id { get; set; }
        [Required, MaxLength(150)]
        public string Nome { get; set; }
        [Required, MaxLength(200)]
        public string Email { get; set; }
        [Required]
        public string PasswordHash { get; set; }
        public string Role { get; set; } = "User";

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public ICollection<Resposta>? Respostas { get; set; }
        public ICollection<Comment>? Comments { get; set; }
    }
}
