using FinanCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanCore.Application.Abstractions;

public interface IFinanCoreDbContext
{
    DbSet<Usuario> Usuarios { get; }
    DbSet<Perfil> Perfis { get; }
    DbSet<Cliente> Clientes { get; }
    DbSet<Fornecedor> Fornecedores { get; }
    DbSet<Categoria> Categorias { get; }
    DbSet<ContaPagar> ContasPagar { get; }
    DbSet<ContaReceber> ContasReceber { get; }
    DbSet<MovimentacaoFinanceira> MovimentacoesFinanceiras { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
