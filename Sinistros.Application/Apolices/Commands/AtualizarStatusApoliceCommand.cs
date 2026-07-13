using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Sinistros.Domain.Exceptions;
using Sinistros.Domain.Apolices;
using Sinistros.Domain.SeedWork;

namespace Sinistros.Application.Apolices.Commands
{
    public record AtualizarStatusApoliceCommand(
        Guid Id,
        string Status
    ) : IRequest<Unit>;

    public class AtualizarStatusApoliceCommandHandler : IRequestHandler<AtualizarStatusApoliceCommand, Unit>
    {
        private readonly IApoliceRepository _apoliceRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AtualizarStatusApoliceCommandHandler(IApoliceRepository apoliceRepository, IUnitOfWork unitOfWork)
        {
            _apoliceRepository = apoliceRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(AtualizarStatusApoliceCommand request, CancellationToken cancellationToken)
        {
            var apolice = await _apoliceRepository.ObterPorIdAsync(request.Id, cancellationToken)
                ?? throw new NotFoundException("Apólice não encontrada.");

            if (request.Status.Equals("Ativa", StringComparison.OrdinalIgnoreCase))
            {
                apolice.Reativar();
            }
            else if (request.Status.Equals("Suspensa", StringComparison.OrdinalIgnoreCase))
            {
                apolice.Suspender();
            }
            else if (request.Status.Equals("Cancelada", StringComparison.OrdinalIgnoreCase))
            {
                apolice.Cancelar();
            }
            else
            {
                throw new RegraNegocioException($"Status '{request.Status}' inválido para atualização de apólice.");
            }

            await _unitOfWork.CommitAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
