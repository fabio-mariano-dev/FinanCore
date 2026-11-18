using FinanCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanCore.Infrastructure.Persistence.Configurations;

public class FornecedorConfiguration : IEntityTypeConfiguration<Fornecedor>
{
    public void Configure(EntityTypeBuilder<Fornecedor> builder)
    {
        builder.ToTable("Fornecedores");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Nome).IsRequired().HasMaxLength(150);
        builder.Property(f => f.NomeFantasia).HasMaxLength(150);
        builder.Property(f => f.DocumentoFiscal).HasMaxLength(20);
        builder.Property(f => f.InscricaoEstadual).HasMaxLength(30);
        builder.Property(f => f.InscricaoMunicipal).HasMaxLength(30);
        builder.Property(f => f.Email).HasMaxLength(200);
        builder.Property(f => f.Telefone).HasMaxLength(20);
        builder.Property(f => f.Celular).HasMaxLength(20);
        builder.Property(f => f.Cep).HasMaxLength(10);
        builder.Property(f => f.Logradouro).HasMaxLength(200);
        builder.Property(f => f.Numero).HasMaxLength(20);
        builder.Property(f => f.Complemento).HasMaxLength(100);
        builder.Property(f => f.Bairro).HasMaxLength(100);
        builder.Property(f => f.Cidade).HasMaxLength(100);
        builder.Property(f => f.Uf).HasMaxLength(2);

        builder.HasIndex(f => f.DocumentoFiscal).IsUnique();
    }
}
