using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Sinistros.Domain.Apolices;
using Sinistros.Domain.Enums;
using Sinistros.Domain.SeedWork;

namespace Sinistros.Application.Apolices.Commands
{
    public record CriarApoliceCommand(
        string Numero,
        Guid ClienteId,
        string Ramo,
        DateTime Inicio,
        DateTime Fim
    ) : IRequest<Guid>;

    public class CriarApoliceCommandHandler : IRequestHandler<CriarApoliceCommand, Guid>
    {
        private readonly IApoliceRepository _apoliceRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CriarApoliceCommandHandler(IApoliceRepository apoliceRepository, IUnitOfWork unitOfWork)
        {
            _apoliceRepository = apoliceRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(CriarApoliceCommand request, CancellationToken cancellationToken)
        {
            if (!Enum.TryParse<Ramo>(request.Ramo, true, out var ramoEnum))
            {
                throw new System.ComponentModel.DataAnnotations.ValidationException("Ramo inválido.");
            }

            var apolice = Apolice.Emitir(
                new NumeroApolice(request.Numero),
                request.ClienteId,
                ramoEnum,
                new PeriodoVigencia(request.Inicio, request.Fim)
            );

            await _apoliceRepository.AdicionarAsync(apolice);
            await _unitOfWork.CommitAsync(cancellationToken);

            return apolice.Id;
        }
    }
}
