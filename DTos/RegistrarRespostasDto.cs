namespace As.Api.Dtos
{
    public class RegistrarRespostasDto
    {
        public int EnqueteId { get; set; }
        public List<RespostaPerguntaDto> Respostas { get; set; }
    }

    public class RespostaPerguntaDto
    {
        public int PerguntaId { get; set; }
        public string Texto { get; set; }
    }
}
