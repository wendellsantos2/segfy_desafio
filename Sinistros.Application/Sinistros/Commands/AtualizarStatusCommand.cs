using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Sinistros.Domain.Exceptions;
using Sinistros.Domain.Enums;
using Sinistros.Domain.Sinistros;
using Sinistros.Domain.SeedWork;

namespace Sinistros.Application.Sinistros.Commands
{
    public record AtualizarStatusCommand(
        Guid Id,
        string Status,
        string Usuario,
        string? MotivoNegativa = null,
        decimal? ValorAprovado = null
    ) : IRequest<Unit>;

    public class AtualizarStatusCommandHandler : IRequestHandler<AtualizarStatusCommand, Unit>
    {
        private readonly ISinistroRepository _sinistroRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AtualizarStatusCommandHandler(ISinistroRepository sinistroRepository, IUnitOfWork unitOfWork)
        {
            _sinistroRepository = sinistroRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(AtualizarStatusCommand request, CancellationToken cancellationToken)
        {
            var sinistro = await _sinistroRepository.ObterPorIdAsync(request.Id)
                ?? throw new NotFoundException("Sinistro não encontrado.");

            if (!Enum.TryParse<StatusSinistro>(request.Status, true, out var statusNovo))
            {
                throw new RegraNegocioException("Status inválido.");
            }

            switch (statusNovo)
            {
                case StatusSinistro.EmAnalise:
                    sinistro.EnviarParaAnalise(request.Usuario);
                    break;
                case StatusSinistro.Aprovado:
                    sinistro.Aprovar(request.Usuario);
                    break;
                case StatusSinistro.Negado:
                    if (string.IsNullOrWhiteSpace(request.MotivoNegativa))
                        throw new RegraNegocioException("O motivo da negativa é obrigatório para negar o sinistro.");
                    sinistro.Negar(new MotivoNegativa(request.MotivoNegativa), request.Usuario);
                    break;
                case StatusSinistro.Encerrado:
                    if (!request.ValorAprovado.HasValue)
                        throw new RegraNegocioException("O valor aprovado é obrigatório para encerrar o sinistro.");
                    sinistro.Encerrar(new Dinheiro(request.ValorAprovado.Value), request.Usuario);
                    break;
                default:
                    throw new RegraNegocioException($"Transição para o status {statusNovo} não suportada por este comando.");
            }

            await _unitOfWork.CommitAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
