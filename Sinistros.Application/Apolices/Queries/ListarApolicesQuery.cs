using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Sinistros.Application.DTOs;
using Sinistros.Domain.Apolices;

namespace Sinistros.Application.Apolices.Queries
{
    public record ListarApolicesQuery(
        string? Status = null,
        int Page = 1,
        int PageSize = 10
    ) : IRequest<PagedResult<ApoliceResponse>>;

    public class ListarApolicesQueryHandler : IRequestHandler<ListarApolicesQuery, PagedResult<ApoliceResponse>>
    {
        private readonly IApoliceRepository _apoliceRepository;

        public ListarApolicesQueryHandler(IApoliceRepository apoliceRepository)
        {
            _apoliceRepository = apoliceRepository;
        }

        public async Task<PagedResult<ApoliceResponse>> Handle(ListarApolicesQuery request, CancellationToken cancellationToken)
        {
            var page = request.Page <= 0 ? 1 : request.Page;
            var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

            var (itens, total) = await _apoliceRepository.ListarAsync(request.Status, page, pageSize, cancellationToken);
            
            var DTOs = itens.Select(ApoliceResponse.Mapear).ToList();

            return new PagedResult<ApoliceResponse>(DTOs, total, page, pageSize);
        }
    }
}
