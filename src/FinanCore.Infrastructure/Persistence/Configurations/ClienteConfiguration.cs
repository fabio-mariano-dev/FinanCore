using FinanCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanCore.Infrastructure.Persistence.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clientes");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nome).IsRequired().HasMaxLength(150);
        builder.Property(c => c.NomeFantasia).HasMaxLength(150);
        builder.Property(c => c.DocumentoFiscal).HasMaxLength(20);
        builder.Property(c => c.InscricaoEstadual).HasMaxLength(30);
        builder.Property(c => c.InscricaoMunicipal).HasMaxLength(30);
        builder.Property(c => c.Email).HasMaxLength(200);
        builder.Property(c => c.Telefone).HasMaxLength(20);
        builder.Property(c => c.Celular).HasMaxLength(20);
        builder.Property(c => c.Cep).HasMaxLength(10);
        builder.Property(c => c.Logradouro).HasMaxLength(200);
        builder.Property(c => c.Numero).HasMaxLength(20);
        builder.Property(c => c.Complemento).HasMaxLength(100);
        builder.Property(c => c.Bairro).HasMaxLength(100);
        builder.Property(c => c.Cidade).HasMaxLength(100);
        builder.Property(c => c.Uf).HasMaxLength(2);

        builder.HasIndex(c => c.DocumentoFiscal).IsUnique();
    }
}
