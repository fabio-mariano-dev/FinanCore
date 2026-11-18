using FinanCore.Application.Abstractions;
using FinanCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanCore.Infrastructure.Persistence;

public class FinanCoreDbContext(DbContextOptions<FinanCoreDbContext> options) : DbContext(options), IFinanCoreDbContext
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Perfil> Perfis => Set<Perfil>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Fornecedor> Fornecedores => Set<Fornecedor>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<ContaPagar> ContasPagar => Set<ContaPagar>();
    public DbSet<ContaReceber> ContasReceber => Set<ContaReceber>();
    public DbSet<MovimentacaoFinanceira> MovimentacoesFinanceiras => Set<MovimentacaoFinanceira>();
    public DbSet<Auditoria> Auditorias => Set<Auditoria>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FinanCoreDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
