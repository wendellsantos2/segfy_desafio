using System;
using Sinistros.Domain.Sinistros;

namespace Sinistros.Application.DTOs
{
    public class SinistroResponse
    {
        public Guid Id { get; set; }
        public Guid ApoliceId { get; set; }
        public DateTime DataOcorrencia { get; set; }
        public DateTime DataAbertura { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public decimal ValorEstimado { get; set; }
        public decimal? ValorAprovado { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? MotivoNegativa { get; set; }
        public DateTime? DataEncerramento { get; set; }

        public static SinistroResponse Mapear(Sinistro sinistro)
        {
            return new SinistroResponse
            {
                Id = sinistro.Id,
                ApoliceId = sinistro.ApoliceId,
                DataOcorrencia = sinistro.DataOcorrencia,
                DataAbertura = sinistro.DataAbertura,
                Descricao = sinistro.Descricao,
                ValorEstimado = sinistro.ValorEstimado.Valor,
                ValorAprovado = sinistro.ValorAprovado?.Valor,
                Status = sinistro.Status.ToString(),
                MotivoNegativa = sinistro.Motivo?.Texto,
                DataEncerramento = sinistro.DataEncerramento
            };
        }
    }
}
