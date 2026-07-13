using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sinistros.Domain.Apolices;

namespace Sinistros.Infrastructure.Persistence.Configurations
{
    public class ApoliceConfiguration : IEntityTypeConfiguration<Apolice>
    {
        public void Configure(EntityTypeBuilder<Apolice> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.ClienteId)
                .IsRequired();

            builder.Property(a => a.Ramo)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(a => a.Status)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(20);

            builder.OwnsOne(a => a.Numero, num =>
            {
                num.Property(n => n.Valor)
                    .IsRequired()
                    .HasMaxLength(50);
                
                num.HasIndex(n => n.Valor).IsUnique();
            });

            builder.OwnsOne(a => a.Vigencia, vig =>
            {
                vig.Property(v => v.Inicio)
                    .IsRequired();
                
                vig.Property(v => v.Fim)
                    .IsRequired();
            });
        }
    }
}
