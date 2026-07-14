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
            var s = await _context.Sinistros
                .Include(x => x.HistoricoSinistros)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (s == null)
                return null;

            return new SinistroResponse
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
                    .OrderByDescending(h => h.DataAlteracao)
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
            };
        }

        public async Task<IReadOnlyList<HistoricoSinistroResponse>?> ObterHistoricoAsync(Guid sinistroId, CancellationToken cancellationToken = default)
        {
            // Verifica existência do sinistro sem carregar o agregado
            var existe = await _context.Sinistros
                .AsNoTracking()
                .AnyAsync(s => s.Id == sinistroId, cancellationToken);

            if (!existe)
                return null;

            return await _context.Set<Sinistros.Domain.Sinistros.HistoricoSinistro>()
                .AsNoTracking()
                .Where(h => h.SinistroId == sinistroId)
                .OrderByDescending(h => h.DataAlteracao)
                .Select(h => new HistoricoSinistroResponse
                {
                    Id = h.Id,
                    StatusAnterior = h.StatusAnterior != null ? h.StatusAnterior.ToString() : null,
                    StatusNovo = h.StatusNovo.ToString(),
                    DataAlteracao = h.DataAlteracao,
                    Motivo = h.Motivo,
                    Usuario = h.Usuario
                })
                .ToListAsync(cancellationToken);
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

            var itemsList = await query
                .OrderByDescending(s => s.DataAbertura)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(s => s.HistoricoSinistros)
                .ToListAsync(cancellationToken);

            var items = itemsList.Select(s => new SinistroResponse
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
                    .OrderByDescending(h => h.DataAlteracao)
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
            }).ToList();

            return new PagedResult<SinistroResponse>(items, totalItems, page, pageSize);
        }
    }
}
