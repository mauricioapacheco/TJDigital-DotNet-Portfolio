using Microsoft.EntityFrameworkCore;
using TJDigital.Api.Domain;

namespace TJDigital.Api.Data;

public sealed class TribunalDbContext(DbContextOptions<TribunalDbContext> options) : DbContext(options)
{
    public DbSet<Processo> Processos => Set<Processo>();
    public DbSet<ParteProcessual> Partes => Set<ParteProcessual>();
    public DbSet<Movimentacao> Movimentacoes => Set<Movimentacao>();
    public DbSet<Audiencia> Audiencias => Set<Audiencia>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Processo>(p =>
        {
            p.HasKey(x => x.Id);
            p.HasIndex(x => x.Numero).IsUnique();
            p.HasIndex(x => x.Status);
            p.HasIndex(x => x.DistribuidoEm);
            p.Property(x => x.Numero).HasMaxLength(40).IsRequired();
            p.Property(x => x.Classe).HasMaxLength(100).IsRequired();
            p.Property(x => x.Assunto).HasMaxLength(180).IsRequired();
            p.Property(x => x.Comarca).HasMaxLength(80).IsRequired();
            p.Property(x => x.Vara).HasMaxLength(100).IsRequired();
            p.Property(x => x.Status).HasMaxLength(32).IsRequired();
            p.HasMany(x => x.Partes).WithOne().HasForeignKey(x => x.ProcessoId).OnDelete(DeleteBehavior.Cascade);
            p.HasMany(x => x.Movimentacoes).WithOne().HasForeignKey(x => x.ProcessoId).OnDelete(DeleteBehavior.Cascade);
            p.HasMany(x => x.Audiencias).WithOne().HasForeignKey(x => x.ProcessoId).OnDelete(DeleteBehavior.Cascade);
        });
        model.Entity<ParteProcessual>(p =>
        {
            p.HasKey(x => x.Id);
            p.Property(x => x.Nome).HasMaxLength(120).IsRequired();
            p.Property(x => x.Polo).HasMaxLength(20).IsRequired();
        });
        model.Entity<Movimentacao>(p =>
        {
            p.HasKey(x => x.Id);
            p.HasIndex(x => new { x.ProcessoId, x.Data });
            p.Property(x => x.Descricao).HasMaxLength(500).IsRequired();
            p.Property(x => x.Responsavel).HasMaxLength(120).IsRequired();
        });
        model.Entity<Audiencia>(p =>
        {
            p.HasKey(x => x.Id);
            p.HasIndex(x => x.Data);
            p.Property(x => x.Tipo).HasMaxLength(80).IsRequired();
            p.Property(x => x.Sala).HasMaxLength(80).IsRequired();
            p.Property(x => x.Status).HasMaxLength(20).IsRequired();
        });
    }
}
