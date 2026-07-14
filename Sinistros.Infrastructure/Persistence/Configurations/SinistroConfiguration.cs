using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sinistros.Domain.Sinistros;

namespace Sinistros.Infrastructure.Persistence.Configurations
{
    public class SinistroConfiguration : IEntityTypeConfiguration<Sinistro>
    {
        public void Configure(EntityTypeBuilder<Sinistro> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.ApoliceId)
                .IsRequired();

            builder.Property(s => s.Descricao)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(s => s.Status)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(s => s.DataOcorrencia)
                .IsRequired();

            builder.Property(s => s.DataAbertura)
                .IsRequired();

            builder.Property(s => s.DataEncerramento);

            builder.OwnsOne(s => s.ValorEstimado, val =>
            {
                val.Property(v => v.Valor)
                    .HasColumnName("valor_estimado_quantia")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();
                
                val.Property(v => v.Moeda)
                    .HasColumnName("valor_estimado_moeda")
                    .HasMaxLength(10)
                    .IsRequired();
            });

            builder.Ignore(s => s.ValorAprovado);
            builder.Ignore(s => s.Motivo);

            builder.Property("_valorAprovadoQuantia")
                .HasColumnName("valor_aprovado_quantia")
                .HasColumnType("decimal(18,2)");

            builder.Property("_valorAprovadoMoeda")
                .HasColumnName("valor_aprovado_moeda")
                .HasMaxLength(10);

            builder.Property("_motivoNegativaTexto")
                .HasColumnName("motivo_negativa_texto")
                .HasMaxLength(500);

            builder.HasMany(s => s.HistoricoSinistros)
                .WithOne()
                .HasForeignKey(h => h.SinistroId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(s => s.HistoricoSinistros)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasField("_historicoSinistros");
        }
    }
}
