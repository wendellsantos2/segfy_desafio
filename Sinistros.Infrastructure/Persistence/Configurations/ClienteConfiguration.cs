using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sinistros.Domain.Clientes;

namespace Sinistros.Infrastructure.Persistence.Configurations
{
    public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
    {
        public void Configure(EntityTypeBuilder<Cliente> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Nome)
                .IsRequired()
                .HasMaxLength(150);

            builder.OwnsOne(c => c.Documento, doc =>
            {
                doc.Property(d => d.Numero)
                    .IsRequired()
                    .HasMaxLength(20);
            });
        }
    }
}
