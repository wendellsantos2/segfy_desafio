using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Sinistros.Application.DTOs;
using Sinistros.Application.Interfaces;
using Sinistros.Domain.Exceptions;

namespace Sinistros.Application.Sinistros.Queries
{
    public record ObterHistoricoSinistroQuery(Guid SinistroId) : IRequest<IReadOnlyList<HistoricoSinistroResponse>>;

    public class ObterHistoricoSinistroQueryHandler
        : IRequestHandler<ObterHistoricoSinistroQuery, IReadOnlyList<HistoricoSinistroResponse>>
    {
        private readonly ISinistroQueries _sinistroQueries;

        public ObterHistoricoSinistroQueryHandler(ISinistroQueries sinistroQueries)
        {
            _sinistroQueries = sinistroQueries;
        }

        public async Task<IReadOnlyList<HistoricoSinistroResponse>> Handle(
            ObterHistoricoSinistroQuery request, CancellationToken cancellationToken)
        {
            var historico = await _sinistroQueries.ObterHistoricoAsync(request.SinistroId, cancellationToken)
                ?? throw new NotFoundException("Sinistro não encontrado.");

            return historico;
        }
    }
}
