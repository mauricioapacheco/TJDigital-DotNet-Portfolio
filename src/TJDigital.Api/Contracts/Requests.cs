namespace TJDigital.Api.Contracts;

public sealed record CriarProcessoRequest(
    string? Classe,
    string? Assunto,
    string? Comarca,
    string? Vara,
    string? Autor,
    string? Reu);

public sealed record AlterarStatusRequest(string? Status);
public sealed record CriarMovimentacaoRequest(string? Descricao, string? Responsavel);
public sealed record CriarAudienciaRequest(int ProcessoId, DateTime Data, string? Tipo, string? Sala);
public sealed record AlterarStatusAudienciaRequest(string? Status);
