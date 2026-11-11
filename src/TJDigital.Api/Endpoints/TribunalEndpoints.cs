using Microsoft.EntityFrameworkCore;
using TJDigital.Api.Contracts;
using TJDigital.Api.Data;
using TJDigital.Api.Domain;

namespace TJDigital.Api.Endpoints;

public static class TribunalEndpoints
{
    public static void MapApiTribunal(this WebApplication app)
    {
        var api = app.MapGroup("/api");

        api.MapGet("/opcoes", () => Results.Ok(new
        {
            classes = Regras.Classes,
            statusProcesso = Regras.StatusProcesso,
            statusAudiencia = Regras.StatusAudiencia
        }));

        api.MapGet("/dashboard", async (TribunalDbContext db) =>
        {
            var agora = DateTime.UtcNow;
            var total = await db.Processos.CountAsync();
            var emAndamento = await db.Processos.CountAsync(p => p.Status == "Em andamento");
            var conclusos = await db.Processos.CountAsync(p => p.Status == "Concluso");
            var arquivados = await db.Processos.CountAsync(p => p.Status == "Arquivado");
            var proximasAudiencias = await db.Audiencias.CountAsync(a => a.Status == "Agendada" && a.Data >= agora && a.Data <= agora.AddDays(30));
            var porClasse = await db.Processos.GroupBy(p => p.Classe)
                .Select(g => new { classe = g.Key, total = g.Count() })
                .OrderByDescending(x => x.total).ToListAsync();
            var recentes = await db.Processos.AsNoTracking().OrderByDescending(p => p.AtualizadoEm).ThenByDescending(p => p.Id).Take(5)
                .Select(p => new { p.Id, p.Numero, p.Classe, p.Assunto, p.Status, p.AtualizadoEm }).ToListAsync();
            return Results.Ok(new { total, emAndamento, conclusos, arquivados, proximasAudiencias, porClasse, recentes });
        });

        api.MapGet("/processos", async (TribunalDbContext db, string? busca, string? status, string? classe, int page = 1, int pageSize = 10) =>
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 50);
            var consulta = db.Processos.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(busca))
            {
                var termo = busca.Trim();
                consulta = consulta.Where(p => p.Numero.Contains(termo) || p.Assunto.Contains(termo) || p.Comarca.Contains(termo));
            }
            if (!string.IsNullOrWhiteSpace(status)) consulta = consulta.Where(p => p.Status == status);
            if (!string.IsNullOrWhiteSpace(classe)) consulta = consulta.Where(p => p.Classe == classe);
            var total = await consulta.CountAsync();
            var registros = await consulta.OrderByDescending(p => p.DistribuidoEm).ThenByDescending(p => p.Id)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(p => new { p.Id, p.Numero, p.Classe, p.Assunto, p.Comarca, p.Vara, p.Status, p.DistribuidoEm, p.AtualizadoEm })
                .ToListAsync();
            return Results.Ok(new { registros, total, pagina = page, porPagina = pageSize, paginas = (int)Math.Ceiling(total / (double)pageSize) });
        });

        api.MapGet("/processos/{id:int}", async Task<IResult> (int id, TribunalDbContext db) =>
        {
            var processo = await db.Processos.AsNoTracking()
                .Include(p => p.Partes).Include(p => p.Movimentacoes).Include(p => p.Audiencias)
                .AsSplitQuery().FirstOrDefaultAsync(p => p.Id == id);
            if (processo is null) return Results.NotFound(new { mensagem = "Processo não encontrado." });
            processo.Movimentacoes = processo.Movimentacoes.OrderByDescending(m => m.Data).ThenByDescending(m => m.Id).ToList();
            processo.Audiencias = processo.Audiencias.OrderBy(a => a.Data).ToList();
            return Results.Ok(processo);
        });

        api.MapPost("/processos", async Task<IResult> (CriarProcessoRequest request, TribunalDbContext db) =>
        {
            var erro = Regras.ValidarProcesso(request);
            if (erro is not null) return Results.BadRequest(new { mensagem = erro });
            var agora = DateTime.UtcNow;
            var processo = new Processo
            {
                Numero = "TEMP-" + Guid.NewGuid().ToString("N"),
                Classe = request.Classe!.Trim(), Assunto = request.Assunto!.Trim(),
                Comarca = request.Comarca!.Trim(), Vara = request.Vara!.Trim(),
                Status = "Em andamento", DistribuidoEm = agora, AtualizadoEm = agora,
                Partes =
                [
                    new ParteProcessual { Nome = request.Autor!.Trim(), Polo = "Autor" },
                    new ParteProcessual { Nome = request.Reu!.Trim(), Polo = "Réu" }
                ],
                Movimentacoes =
                [
                    new Movimentacao { Data = agora, Descricao = "Distribuição registrada no sistema demonstrativo.", Responsavel = "Sistema TJ Digital" }
                ]
            };
            db.Processos.Add(processo);
            await db.SaveChangesAsync();
            processo.Numero = $"DEMO-{agora:yyyy}-{processo.Id:0000}";
            await db.SaveChangesAsync();
            return Results.Created($"/api/processos/{processo.Id}", new { processo.Id, processo.Numero });
        });

        api.MapPut("/processos/{id:int}/status", async Task<IResult> (int id, AlterarStatusRequest request, TribunalDbContext db) =>
        {
            if (!Regras.StatusValido(request.Status)) return Results.BadRequest(new { mensagem = "Status processual inválido." });
            var processo = await db.Processos.FindAsync(id);
            if (processo is null) return Results.NotFound(new { mensagem = "Processo não encontrado." });
            processo.Status = request.Status!;
            processo.AtualizadoEm = DateTime.UtcNow;
            db.Movimentacoes.Add(new Movimentacao
            {
                ProcessoId = id, Data = processo.AtualizadoEm, Descricao = $"Situação alterada para: {processo.Status}.",
                Responsavel = "Sistema TJ Digital"
            });
            await db.SaveChangesAsync();
            return Results.Ok(new { processo.Id, processo.Status });
        });

        api.MapPost("/processos/{id:int}/movimentacoes", async Task<IResult> (int id, CriarMovimentacaoRequest request, TribunalDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(request.Descricao) || request.Descricao.Trim().Length is < 5 or > 500)
                return Results.BadRequest(new { mensagem = "A movimentação deve conter entre 5 e 500 caracteres." });
            if (string.IsNullOrWhiteSpace(request.Responsavel) || request.Responsavel.Trim().Length > 120)
                return Results.BadRequest(new { mensagem = "Informe o responsável com até 120 caracteres." });
            var processo = await db.Processos.FindAsync(id);
            if (processo is null) return Results.NotFound(new { mensagem = "Processo não encontrado." });
            var agora = DateTime.UtcNow;
            var movimento = new Movimentacao
            {
                ProcessoId = id, Data = agora, Descricao = request.Descricao.Trim(), Responsavel = request.Responsavel.Trim()
            };
            processo.AtualizadoEm = agora;
            db.Movimentacoes.Add(movimento);
            await db.SaveChangesAsync();
            return Results.Created($"/api/processos/{id}", new { movimento.Id, movimento.Data });
        });

        api.MapGet("/audiencias", async (TribunalDbContext db) =>
        {
            var dados = await (from a in db.Audiencias.AsNoTracking()
                join p in db.Processos.AsNoTracking() on a.ProcessoId equals p.Id
                orderby a.Data
                select new { a.Id, a.ProcessoId, p.Numero, p.Assunto, p.Comarca, a.Tipo, a.Sala, a.Data, a.Status })
                .ToListAsync();
            return Results.Ok(dados);
        });

        api.MapPost("/audiencias", async Task<IResult> (CriarAudienciaRequest request, TribunalDbContext db) =>
        {
            if (request.Data <= DateTime.UtcNow.AddMinutes(1))
                return Results.BadRequest(new { mensagem = "A audiência deve ser agendada para uma data futura." });
            if (string.IsNullOrWhiteSpace(request.Tipo) || request.Tipo.Trim().Length > 80 ||
                string.IsNullOrWhiteSpace(request.Sala) || request.Sala.Trim().Length > 80)
                return Results.BadRequest(new { mensagem = "Preencha o tipo e a sala (até 80 caracteres)." });
            if (!await db.Processos.AnyAsync(p => p.Id == request.ProcessoId))
                return Results.BadRequest(new { mensagem = "Processo inválido." });
            var audiencia = new Audiencia
            {
                ProcessoId = request.ProcessoId, Data = request.Data, Tipo = request.Tipo.Trim(),
                Sala = request.Sala.Trim(), Status = "Agendada"
            };
            db.Audiencias.Add(audiencia);
            await db.SaveChangesAsync();
            return Results.Created($"/api/audiencias/{audiencia.Id}", new { audiencia.Id });
        });

        api.MapPatch("/audiencias/{id:int}/status", async Task<IResult> (int id, AlterarStatusAudienciaRequest request, TribunalDbContext db) =>
        {
            if (!Regras.StatusAudienciaValido(request.Status))
                return Results.BadRequest(new { mensagem = "Situação da audiência inválida." });
            var audiencia = await db.Audiencias.FindAsync(id);
            if (audiencia is null) return Results.NotFound(new { mensagem = "Audiência não encontrada." });
            audiencia.Status = request.Status!;
            await db.SaveChangesAsync();
            return Results.Ok(new { audiencia.Id, audiencia.Status });
        });
    }
}
