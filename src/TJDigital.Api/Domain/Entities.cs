namespace TJDigital.Api.Domain;

public sealed class Processo
{
    public int Id { get; set; }
    public string Numero { get; set; } = "";
    public string Classe { get; set; } = "";
    public string Assunto { get; set; } = "";
    public string Comarca { get; set; } = "";
    public string Vara { get; set; } = "";
    public string Status { get; set; } = "Em andamento";
    public DateTime DistribuidoEm { get; set; }
    public DateTime AtualizadoEm { get; set; }
    public List<ParteProcessual> Partes { get; set; } = [];
    public List<Movimentacao> Movimentacoes { get; set; } = [];
    public List<Audiencia> Audiencias { get; set; } = [];
}

public sealed class ParteProcessual
{
    public int Id { get; set; }
    public int ProcessoId { get; set; }
    public string Nome { get; set; } = "";
    public string Polo { get; set; } = "";
}

public sealed class Movimentacao
{
    public int Id { get; set; }
    public int ProcessoId { get; set; }
    public DateTime Data { get; set; }
    public string Descricao { get; set; } = "";
    public string Responsavel { get; set; } = "";
}

public sealed class Audiencia
{
    public int Id { get; set; }
    public int ProcessoId { get; set; }
    public DateTime Data { get; set; }
    public string Tipo { get; set; } = "";
    public string Sala { get; set; } = "";
    public string Status { get; set; } = "Agendada";
}
