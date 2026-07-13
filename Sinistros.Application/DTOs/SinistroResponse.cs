using System;
using System.Collections.Generic;
using System.Linq;
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
        public List<HistoricoSinistroResponse> Historico { get; set; } = new();

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
                DataEncerramento = sinistro.DataEncerramento,
                Historico = sinistro.HistoricoSinistros
                    .Select(h => new HistoricoSinistroResponse
                    {
                        Id = h.Id,
                        StatusAnterior = h.StatusAnterior?.ToString(),
                        StatusNovo = h.StatusNovo.ToString(),
                        DataAlteracao = h.DataAlteracao,
                        Motivo = h.Motivo,
                        Usuario = h.Usuario
                    })
                    .ToList()
            };
        }
    }

    public class HistoricoSinistroResponse
    {
        public Guid Id { get; set; }
        public string? StatusAnterior { get; set; }
        public string StatusNovo { get; set; } = string.Empty;
        public DateTime DataAlteracao { get; set; }
        public string? Motivo { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }
}
