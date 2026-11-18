using FinanCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanCore.Infrastructure.Persistence.Configurations;

public class MovimentacaoFinanceiraConfiguration : IEntityTypeConfiguration<MovimentacaoFinanceira>
{
    public void Configure(EntityTypeBuilder<MovimentacaoFinanceira> builder)
    {
        builder.ToTable("MovimentacoesFinanceiras");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Valor).HasColumnType("decimal(18,2)");
        builder.Property(m => m.Tipo).IsRequired().HasConversion<string>().HasMaxLength(10);

        builder.HasIndex(m => m.DataMovimentacao);
        builder.HasIndex(m => m.CategoriaId);

        builder.HasIndex(m => m.ContaPagarId).IsUnique();
        builder.HasIndex(m => m.ContaReceberId).IsUnique();

        builder.HasOne<ContaPagar>().WithMany().HasForeignKey(m => m.ContaPagarId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ContaReceber>().WithMany().HasForeignKey(m => m.ContaReceberId).OnDelete(DeleteBehavior.Restrict);
    }
}
