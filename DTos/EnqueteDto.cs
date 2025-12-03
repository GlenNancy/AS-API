using As.Api.Dtos;

public class EnqueteDto
{
    public int Id { get; set; }
    public string Titulo { get; set; }
    public string Descricao { get; set; }
    public DateTime DataCriacao { get; set; }
    public bool ContaParaAcesso { get; set; }

    public List<PerguntaDto> Perguntas { get; set; }
}
