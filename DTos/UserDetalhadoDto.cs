namespace As.Api.Dtos
{
    public class UserDetalhadoDto
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public List<RespostaDto> Respostas { get; set; }
        public List<CommentDto> Comments { get; set; }
    }
}
