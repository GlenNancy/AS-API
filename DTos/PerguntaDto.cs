namespace As.Api.Dtos
{
    public class PerguntaDto
    {
        public int Id { get; set; }
        public string Texto { get; set; }
        public bool Obrigatoria { get; set; } 

        public List<RespostaDto> Respostas { get; set; }
    }
}
