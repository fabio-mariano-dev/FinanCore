using FinanCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanCore.Infrastructure.Persistence.Configurations;

public class AuditoriaConfiguration : IEntityTypeConfiguration<Auditoria>
{
    public void Configure(EntityTypeBuilder<Auditoria> builder)
    {
        builder.ToTable("Auditorias");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Entidade).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Acao).IsRequired().HasMaxLength(50);

        builder.HasIndex(a => new { a.Entidade, a.EntidadeId });
        builder.HasIndex(a => a.DataHora);
    }
}
