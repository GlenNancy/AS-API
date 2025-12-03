namespace As.Api.Dtos
{
    public class RecompensaDto
    {
        public string Login { get; set; }
        public string Senha { get; set; } // senha em texto plano retornada apenas no momento
        public DateTime DataGeracao { get; set; }
    }
}
