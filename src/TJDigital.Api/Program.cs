using Microsoft.EntityFrameworkCore;
using TJDigital.Api.Data;
using TJDigital.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

var provider = builder.Configuration["Database:Provider"] ?? "Sqlite";
builder.Services.AddDbContext<TribunalDbContext>(options =>
{
    if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
    {
        var connection = builder.Configuration.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("Configure ConnectionStrings:SqlServer para usar SQL Server.");
        options.UseSqlServer(connection);
    }
    else if (provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
    {
        options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite") ?? "Data Source=tjdigital.db");
    }
    else
    {
        throw new InvalidOperationException("Provedor inválido. Use Sqlite ou SqlServer.");
    }
});

builder.Services.AddProblemDetails();
var app = builder.Build();

app.UseExceptionHandler();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/health", () => Results.Ok(new { status = "ok", ambiente = "demonstracao", utc = DateTime.UtcNow }));
app.MapApiTribunal();

using (var scope = app.Services.CreateScope())
    await DadosDemonstracao.InicializarAsync(scope.ServiceProvider.GetRequiredService<TribunalDbContext>());

app.Run();

public partial class Program { }
