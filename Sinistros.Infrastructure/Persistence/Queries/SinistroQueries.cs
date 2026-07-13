using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sinistros.Application.DTOs;
using Sinistros.Application.Interfaces;
using Sinistros.Domain.Enums;

namespace Sinistros.Infrastructure.Persistence.Queries
{
    public class SinistroQueries : ISinistroQueries
    {
        private readonly AppDbContext _context;

        public SinistroQueries(AppDbContext context)
        {
            _context = context;
        }

        public async Task<SinistroResponse?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Sinistros
                .AsNoTracking()
                .Where(s => s.Id == id)
                .Select(s => new SinistroResponse
                {
                    Id = s.Id,
                    ApoliceId = s.ApoliceId,
                    DataOcorrencia = s.DataOcorrencia,
                    DataAbertura = s.DataAbertura,
                    Descricao = s.Descricao,
                    ValorEstimado = s.ValorEstimado.Valor,
                    ValorAprovado = s.ValorAprovado != null ? s.ValorAprovado.Valor : (decimal?)null,
                    Status = s.Status.ToString(),
                    MotivoNegativa = s.Motivo != null ? s.Motivo.Texto : null,
                    DataEncerramento = s.DataEncerramento,
                    Historico = s.HistoricoSinistros
                        .Select(h => new HistoricoSinistroResponse
                        {
                            Id = h.Id,
                            StatusAnterior = h.StatusAnterior != null ? h.StatusAnterior.ToString() : null,
                            StatusNovo = h.StatusNovo.ToString(),
                            DataAlteracao = h.DataAlteracao,
                            Motivo = h.Motivo,
                            Usuario = h.Usuario
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<PagedResult<SinistroResponse>> ListarAsync(
            string? status,
            DateTime? dataInicio,
            DateTime? dataFim,
            string? campoData,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Sinistros.AsNoTracking().AsQueryable();

            // Filtro por Status
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (Enum.TryParse<StatusSinistro>(status, true, out var statusEnum))
                {
                    query = query.Where(s => s.Status == statusEnum);
                }
                else
                {
                    return new PagedResult<SinistroResponse>(new List<SinistroResponse>(), 0, page, pageSize);
                }
            }

            // Filtro por período dinâmico (abertura vs ocorrencia)
            if (dataInicio.HasValue || dataFim.HasValue)
            {
                var filtrarPorOcorrencia = !string.IsNullOrWhiteSpace(campoData) && 
                                           campoData.Equals("ocorrencia", StringComparison.OrdinalIgnoreCase);

                if (filtrarPorOcorrencia)
                {
                    if (dataInicio.HasValue)
                        query = query.Where(s => s.DataOcorrencia >= dataInicio.Value);
                    if (dataFim.HasValue)
                        query = query.Where(s => s.DataOcorrencia <= dataFim.Value);
                }
                else // Padrão ou "abertura"
                {
                    if (dataInicio.HasValue)
                        query = query.Where(s => s.DataAbertura >= dataInicio.Value);
                    if (dataFim.HasValue)
                        query = query.Where(s => s.DataAbertura <= dataFim.Value);
                }
            }

            var totalItems = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(s => s.DataAbertura)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new SinistroResponse
                {
                    Id = s.Id,
                    ApoliceId = s.ApoliceId,
                    DataOcorrencia = s.DataOcorrencia,
                    DataAbertura = s.DataAbertura,
                    Descricao = s.Descricao,
                    ValorEstimado = s.ValorEstimado.Valor,
                    ValorAprovado = s.ValorAprovado != null ? s.ValorAprovado.Valor : (decimal?)null,
                    Status = s.Status.ToString(),
                    MotivoNegativa = s.Motivo != null ? s.Motivo.Texto : null,
                    DataEncerramento = s.DataEncerramento,
                    Historico = s.HistoricoSinistros
                        .Select(h => new HistoricoSinistroResponse
                        {
                            Id = h.Id,
                            StatusAnterior = h.StatusAnterior != null ? h.StatusAnterior.ToString() : null,
                            StatusNovo = h.StatusNovo.ToString(),
                            DataAlteracao = h.DataAlteracao,
                            Motivo = h.Motivo,
                            Usuario = h.Usuario
                        })
                        .ToList()
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<SinistroResponse>(items, totalItems, page, pageSize);
        }
    }
}
