using Microsoft.EntityFrameworkCore;
using TJDigital.Api.Domain;

namespace TJDigital.Api.Data;

public static class DadosDemonstracao
{
    public static async Task InicializarAsync(TribunalDbContext db)
    {
        await db.Database.EnsureCreatedAsync();
        if (await db.Processos.AnyAsync()) return;

        var assuntos = new[]
        {
            "Revisão contratual de prestação de serviços", "Cobrança de obrigação comercial", "Indenização por danos materiais",
            "Cumprimento de acordo homologado", "Rescisão contratual", "Obrigação de fazer",
            "Direito de vizinhança", "Restituição de valores", "Revisão de cláusulas contratuais",
            "Dissolução consensual", "Execução de obrigação", "Responsabilidade civil",
            "Direito do consumidor", "Tutela de urgência", "Inventário consensual",
            "Reparação de prejuízos", "Conciliação entre partes", "Pedido de cumprimento de sentença"
        };
        var comarcas = new[] { "Comarca Modelo Norte", "Comarca Modelo Central", "Comarca Modelo Sul" };
        var status = new[] { "Em andamento", "Em andamento", "Concluso", "Em andamento", "Suspenso", "Arquivado" };
        var hoje = DateTime.UtcNow.Date;

        for (var i = 0; i < assuntos.Length; i++)
        {
            var data = hoje.AddDays(-i * 11 - 4);
            var processo = new Processo
            {
                Numero = $"DEMO-2026-{i + 1:0000}",
                Classe = Regras.Classes[i % Regras.Classes.Length],
                Assunto = assuntos[i],
                Comarca = comarcas[i % comarcas.Length],
                Vara = $"{i % 4 + 1}ª Vara Demonstrativa",
                Status = status[i % status.Length],
                DistribuidoEm = data,
                AtualizadoEm = hoje.AddDays(-i % 8),
                Partes =
                [
                    new ParteProcessual { Polo = "Autor", Nome = $"Parte Autora Exemplo {i + 1:00}" },
                    new ParteProcessual { Polo = "Réu", Nome = $"Parte Ré Exemplo {i + 1:00}" }
                ],
                Movimentacoes =
                [
                    new Movimentacao { Data = data, Descricao = "Distribuição processual registrada no ambiente demonstrativo.", Responsavel = "Secretaria de Demonstração" },
                    new Movimentacao { Data = data.AddDays(2), Descricao = "Autuação e conferência inicial dos documentos fictícios.", Responsavel = "Unidade Judiciária Simulada" },
                    new Movimentacao { Data = data.AddDays(7), Descricao = "Despacho inicial disponibilizado para consulta simulada.", Responsavel = "Gabinete Demonstrativo" }
                ]
            };
            if (i < 12)
                processo.Audiencias.Add(new Audiencia
                {
                    Data = hoje.AddDays((i + 1) * 3).AddHours(9 + i % 6),
                    Tipo = i % 2 == 0 ? "Conciliação" : "Instrução",
                    Sala = $"Sala Virtual {i % 3 + 1}",
                    Status = "Agendada"
                });
            db.Processos.Add(processo);
        }
        await db.SaveChangesAsync();
    }
}
