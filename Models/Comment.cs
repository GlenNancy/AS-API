using System;

namespace As.Api.Models {
    public class Comment {
        public int Id { get; set; }
        public int EnqueteId { get; set; }
        public int UserId { get; set; }
        public string Texto { get; set; }
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        public Enquete? Enquete { get; set; }
        public User? User { get; set; }
    }
}
