using TJDigital.Api.Contracts;
using TJDigital.Api.Domain;
using Xunit;

namespace TJDigital.Tests;

public sealed class RegrasTests
{
    [Fact]
    public void DeveAceitarCadastroValido()
    {
        var req = new CriarProcessoRequest(Regras.Classes[0], "Revisão contratual demonstrativa", "Comarca Modelo", "1ª Vara Demonstrativa", "Parte Autora 1", "Parte Ré 1");
        Assert.Null(Regras.ValidarProcesso(req));
    }

    [Theory]
    [InlineData("")]
    [InlineData("Abc")]
    public void DeveRecusarAssuntoMuitoCurto(string assunto)
    {
        var req = new CriarProcessoRequest(Regras.Classes[0], assunto, "Comarca Modelo", "1ª Vara", "Autor", "Réu");
        Assert.NotNull(Regras.ValidarProcesso(req));
    }

    [Fact]
    public void DeveRecusarClasseDesconhecida()
    {
        var req = new CriarProcessoRequest("Classe não cadastrada", "Assunto demonstrativo", "Comarca Modelo", "1ª Vara", "Autor", "Réu");
        Assert.NotNull(Regras.ValidarProcesso(req));
    }

    [Theory]
    [InlineData("Em andamento", true)]
    [InlineData("Concluso", true)]
    [InlineData("Arquivado", true)]
    [InlineData("Apagado", false)]
    public void DeveValidarSituacoes(string status, bool esperado)
    {
        Assert.Equal(esperado, Regras.StatusValido(status));
    }

    [Theory]
    [InlineData("Agendada", true)]
    [InlineData("Realizada", true)]
    [InlineData("Cancelada", true)]
    [InlineData("Pendente de envio", false)]
    public void DeveValidarSituacoesDeAudiencia(string status, bool esperado)
    {
        Assert.Equal(esperado, Regras.StatusAudienciaValido(status));
    }
}
