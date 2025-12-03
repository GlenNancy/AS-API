namespace As.Api.Dtos
{
    public class EnqueteDetalhadaDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Descricao { get; set; }
        public DateTime DataCriacao { get; set; }
        public bool ContaParaAcesso { get; set; }

        public List<PerguntaDto> Perguntas { get; set; }
        public List<CommentDto> Comments { get; set; }
    }
}
