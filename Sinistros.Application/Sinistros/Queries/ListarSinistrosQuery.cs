using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Sinistros.Application.DTOs;
using Sinistros.Application.Interfaces;

namespace Sinistros.Application.Sinistros.Queries
{
    public record ListarSinistrosQuery(
        string? Status = null,
        DateTime? DataInicio = null,
        DateTime? DataFim = null,
        string? CampoData = null,
        int Page = 1,
        int PageSize = 10
    ) : IRequest<PagedResult<SinistroResponse>>;

    public class ListarSinistrosQueryHandler : IRequestHandler<ListarSinistrosQuery, PagedResult<SinistroResponse>>
    {
        private readonly ISinistroQueries _sinistroQueries;

        public ListarSinistrosQueryHandler(ISinistroQueries sinistroQueries)
        {
            _sinistroQueries = sinistroQueries;
        }

        public async Task<PagedResult<SinistroResponse>> Handle(ListarSinistrosQuery request, CancellationToken cancellationToken)
        {
            var page = request.Page <= 0 ? 1 : request.Page;
            var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

            return await _sinistroQueries.ListarAsync(
                request.Status,
                request.DataInicio,
                request.DataFim,
                request.CampoData,
                page,
                pageSize,
                cancellationToken);
        }
    }
}
