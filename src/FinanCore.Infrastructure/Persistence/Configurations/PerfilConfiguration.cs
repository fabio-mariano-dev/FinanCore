using FinanCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanCore.Infrastructure.Persistence.Configurations;

public class PerfilConfiguration : IEntityTypeConfiguration<Perfil>
{
    public void Configure(EntityTypeBuilder<Perfil> builder)
    {
        builder.ToTable("Perfis");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Nome).IsRequired().HasMaxLength(50);
    }
}
