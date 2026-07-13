using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Sinistros.Application.DTOs;
using Sinistros.Application.Interfaces;
using Sinistros.Domain.Exceptions;

namespace Sinistros.Application.Sinistros.Queries
{
    public record ObterSinistroQuery(Guid Id) : IRequest<SinistroResponse>;

    public class ObterSinistroQueryHandler : IRequestHandler<ObterSinistroQuery, SinistroResponse>
    {
        private readonly ISinistroQueries _sinistroQueries;

        public ObterSinistroQueryHandler(ISinistroQueries sinistroQueries)
        {
            _sinistroQueries = sinistroQueries;
        }

        public async Task<SinistroResponse> Handle(ObterSinistroQuery request, CancellationToken cancellationToken)
        {
            var sinistro = await _sinistroQueries.ObterPorIdAsync(request.Id, cancellationToken)
                ?? throw new NotFoundException("Sinistro não encontrado.");

            return sinistro;
        }
    }
}
