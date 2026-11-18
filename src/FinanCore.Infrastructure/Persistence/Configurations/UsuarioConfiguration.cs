using FinanCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanCore.Infrastructure.Persistence.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Nome).IsRequired().HasMaxLength(150);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(200);
        builder.Property(u => u.SenhaHash).IsRequired();

        builder.HasIndex(u => u.Email).IsUnique();

        builder.HasOne<Perfil>().WithMany().HasForeignKey(u => u.PerfilId).OnDelete(DeleteBehavior.Restrict);
    }
}
