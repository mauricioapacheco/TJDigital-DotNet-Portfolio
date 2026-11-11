using TJDigital.Api.Contracts;

namespace TJDigital.Api.Domain;

public static class Regras
{
    public static readonly string[] StatusProcesso =
        ["Em andamento", "Concluso", "Suspenso", "Arquivado"];

    public static readonly string[] Classes =
        ["Procedimento Comum Cível", "Cumprimento de Sentença", "Juizado Especial Cível", "Ação de Família", "Execução de Título Extrajudicial"];

    public static readonly string[] StatusAudiencia =
        ["Agendada", "Realizada", "Cancelada"];

    public static string? ValidarProcesso(CriarProcessoRequest requisicao)
    {
        if (!Classes.Contains(requisicao.Classe)) return "Selecione uma classe processual válida.";
        if (string.IsNullOrWhiteSpace(requisicao.Assunto) || requisicao.Assunto.Trim().Length is < 5 or > 180)
            return "O assunto deve conter entre 5 e 180 caracteres.";
        if (string.IsNullOrWhiteSpace(requisicao.Comarca) || requisicao.Comarca.Trim().Length > 80)
            return "Informe uma comarca com até 80 caracteres.";
        if (string.IsNullOrWhiteSpace(requisicao.Vara) || requisicao.Vara.Trim().Length > 100)
            return "Informe uma vara com até 100 caracteres.";
        if (string.IsNullOrWhiteSpace(requisicao.Autor) || requisicao.Autor.Trim().Length > 120)
            return "Informe a parte autora com até 120 caracteres.";
        if (string.IsNullOrWhiteSpace(requisicao.Reu) || requisicao.Reu.Trim().Length > 120)
            return "Informe a parte ré com até 120 caracteres.";
        return null;
    }

    public static bool StatusValido(string? status) => StatusProcesso.Contains(status);
    public static bool StatusAudienciaValido(string? status) => StatusAudiencia.Contains(status);
}
