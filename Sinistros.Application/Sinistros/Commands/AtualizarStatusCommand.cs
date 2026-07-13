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
                throw new RegraNegocioException($"Status '{request.Status}' é inválido.");

            switch (statusNovo)
            {
                case StatusSinistro.EmAnalise:
                    sinistro.EnviarParaAnalise(request.Usuario);
                    break;
                case StatusSinistro.Aprovado:
                    sinistro.Aprovar(request.Usuario);
                    break;
                case StatusSinistro.Negado:
                    // MotivoNegativa VO lança RegraNegocioException se motivo for vazio
                    sinistro.Negar(new MotivoNegativa(request.MotivoNegativa ?? string.Empty), request.Usuario);
                    break;
                case StatusSinistro.Encerrado:
                    // Dinheiro VO lança RegraNegocioException se valor for inválido
                    sinistro.Encerrar(new Dinheiro(request.ValorAprovado ?? 0m), request.Usuario);
                    break;
                default:
                    throw new RegraNegocioException($"Transição para '{statusNovo}' não suportada por este comando.");
            }

            await _unitOfWork.CommitAsync(cancellationToken);
            return Unit.Value;
        }
    }
}
