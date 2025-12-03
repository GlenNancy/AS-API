using As.Api.Dtos;

public class CommentDto
    {
        public int Id { get; set; }
        public string Texto { get; set; }
        public DateTime DataCriacao { get; set; }

        public UserDto User { get; set; }
    }
