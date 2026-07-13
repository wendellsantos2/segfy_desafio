using System;
using System.Threading;
using System.Threading.Tasks;
using Sinistros.Application.DTOs;

namespace Sinistros.Application.Interfaces
{
    public interface ISinistroQueries
    {
        Task<SinistroResponse?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<HistoricoSinistroResponse>?> ObterHistoricoAsync(Guid sinistroId, CancellationToken cancellationToken = default);

        Task<PagedResult<SinistroResponse>> ListarAsync(
            string? status,
            DateTime? dataInicio,
            DateTime? dataFim,
            string? campoData,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);
    }
}
