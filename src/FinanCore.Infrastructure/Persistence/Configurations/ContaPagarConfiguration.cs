using FinanCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanCore.Infrastructure.Persistence.Configurations;

public class ContaPagarConfiguration : IEntityTypeConfiguration<ContaPagar>
{
    public void Configure(EntityTypeBuilder<ContaPagar> builder)
    {
        builder.ToTable("ContasPagar");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Descricao).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Valor).HasColumnType("decimal(18,2)");
        builder.Property(c => c.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.Observacoes).HasMaxLength(500);

        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasIndex(c => new { c.Status, c.DataVencimento });
        builder.HasIndex(c => c.FornecedorId);
        builder.HasIndex(c => c.CategoriaId);

        builder.HasOne<Fornecedor>().WithMany().HasForeignKey(c => c.FornecedorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Categoria>().WithMany().HasForeignKey(c => c.CategoriaId).OnDelete(DeleteBehavior.Restrict);
    }
}
