using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Sinistros.Application.DTOs;
using Sinistros.Domain.Apolices;
using Sinistros.Domain.Exceptions;

namespace Sinistros.Application.Apolices.Queries
{
    public record ObterApolicePorIdQuery(Guid Id) : IRequest<ApoliceResponse>;

    public class ObterApolicePorIdQueryHandler : IRequestHandler<ObterApolicePorIdQuery, ApoliceResponse>
    {
        private readonly IApoliceRepository _apoliceRepository;

        public ObterApolicePorIdQueryHandler(IApoliceRepository apoliceRepository)
        {
            _apoliceRepository = apoliceRepository;
        }

        public async Task<ApoliceResponse> Handle(ObterApolicePorIdQuery request, CancellationToken cancellationToken)
        {
            var apolice = await _apoliceRepository.ObterPorIdAsync(request.Id, cancellationToken)
                ?? throw new NotFoundException("Apólice não encontrada.");

            return ApoliceResponse.Mapear(apolice);
        }
    }
}
