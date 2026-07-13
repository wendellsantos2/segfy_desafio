using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Sinistros.Application.DTOs;
using Sinistros.Domain.Apolices;
using Sinistros.Domain.Exceptions;
using Sinistros.Domain.SeedWork;
using Sinistros.Domain.Servicos;
using Sinistros.Domain.Sinistros;

namespace Sinistros.Application.Sinistros.Commands
{
    public class AbrirSinistroCommandHandler : IRequestHandler<AbrirSinistroCommand, SinistroResponse>
    {
        private readonly IApoliceRepository _apoliceRepository;
        private readonly ISinistroRepository _sinistroRepository;
        private readonly AberturaDeSinistroService _aberturaDeSinistroService;
        private readonly IUnitOfWork _unitOfWork;

        public AbrirSinistroCommandHandler(
            IApoliceRepository apoliceRepository,
            ISinistroRepository sinistroRepository,
            AberturaDeSinistroService aberturaDeSinistroService,
            IUnitOfWork unitOfWork)
        {
            _apoliceRepository = apoliceRepository;
            _sinistroRepository = sinistroRepository;
            _aberturaDeSinistroService = aberturaDeSinistroService;
            _unitOfWork = unitOfWork;
        }

        public async Task<SinistroResponse> Handle(AbrirSinistroCommand request, CancellationToken cancellationToken)
        {
            var apolice = await _apoliceRepository.ObterPorIdAsync(request.ApoliceId) ?? throw new NotFoundException("Apólice não encontrada.");
            var sinistro = _aberturaDeSinistroService.Abrir(apolice, request.DataOcorrencia, request.Descricao, new Dinheiro(request.ValorEstimado));
            await _sinistroRepository.AdicionarAsync(sinistro);
            await _unitOfWork.CommitAsync(cancellationToken);
            return SinistroResponse.Mapear(sinistro);
        }
    }
}
