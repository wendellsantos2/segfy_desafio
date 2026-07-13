using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Sinistros.Application.Sinistros.Commands;
using Sinistros.Domain.Apolices;
using Sinistros.Domain.Enums;
using Sinistros.Domain.Exceptions;
using Sinistros.Domain.SeedWork;
using Sinistros.Domain.Servicos;
using Sinistros.Domain.Sinistros;
using Xunit;

namespace Sinistros.Tests.Application
{
    /// <summary>
    /// Passo 26 - Testes de handlers com mocks de repositório e UnitOfWork (NSubstitute).
    ///
    /// POR QUE mockamos repositório e UoW, mas NUNCA o domínio?
    /// ─────────────────────────────────────────────────────────
    /// O repositório e o UoW são INFRAESTRUTURA — dependências externas (banco, rede)
    /// que não queremos acionar em testes unitários. Substituí-los por dublês nos permite
    /// testar a orquestração do handler de forma rápida, determinista e isolada.
    ///
    /// O DOMÍNIO, por sua vez, é código puro sem dependências externas: ele não faz I/O,
    /// não fala com banco e pode ser instanciado diretamente. Mockar o domínio seria
    /// ignorar exatamente o que estamos testando — as regras de negócio. Se mockássemos
    /// Sinistro.Abrir(), poderíamos passar um teste com lógica de domínio quebrada.
    /// </summary>
    public class AbrirSinistroHandlerApplicationTests
    {
        private readonly IApoliceRepository _apoliceRepository = Substitute.For<IApoliceRepository>();
        private readonly ISinistroRepository _sinistroRepository = Substitute.For<ISinistroRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly AberturaDeSinistroService _aberturaDeSinistroService = new();
        private readonly AbrirSinistroCommandHandler _handler;

        public AbrirSinistroHandlerApplicationTests()
        {
            _handler = new AbrirSinistroCommandHandler(
                _apoliceRepository,
                _sinistroRepository,
                _aberturaDeSinistroService,
                _unitOfWork);
        }

        // ─── Helpers ────────────────────────────────────────────────────────────────

        private static Apolice CriarApoliceAtiva(Guid? id = null)
        {
            var apolice = Apolice.Emitir(
                new NumeroApolice("ABCD-123456"),
                Guid.NewGuid(),
                Ramo.Auto,
                new PeriodoVigencia(DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow.AddMonths(11)));

            if (id.HasValue)
                ReflectionHelper.SetPrivateId(apolice, id.Value);

            return apolice;
        }

        private static AbrirSinistroCommand ComandoValido(Guid apoliceId) =>
            new(apoliceId, DateTime.UtcNow.AddDays(-2), "Batida traseira em rodovia federal.", 5000m);

        // ─── NotFoundException quando apólice não existe ────────────────────────────

        [Fact]
        public async Task Handle_ApoliceNaoExiste_LancaNotFoundException()
        {
            // Arrange
            var apoliceId = Guid.NewGuid();
            _apoliceRepository.ObterPorIdAsync(apoliceId).Returns((Apolice)null!);

            // Act
            var ato = () => _handler.Handle(ComandoValido(apoliceId), CancellationToken.None);

            // Assert
            await ato.Should().ThrowAsync<NotFoundException>()
                .WithMessage("*Apólice*");

            await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
        }

        // ─── CommitAsync chamado exatamente 1 vez no caminho feliz ─────────────────

        [Fact]
        public async Task Handle_CaminhoFeliz_ChamaCommitAsyncExatamente1Vez()
        {
            // Arrange
            var apoliceId = Guid.NewGuid();
            var apolice = CriarApoliceAtiva(apoliceId);
            _apoliceRepository.ObterPorIdAsync(apoliceId).Returns(apolice);

            // Act
            await _handler.Handle(ComandoValido(apoliceId), CancellationToken.None);

            // Assert
            await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
        }

        // ─── CommitAsync NÃO chamado quando domínio lança exceção ──────────────────

        [Fact]
        public async Task Handle_ApoliceInativa_NaoChamaCommitAsync()
        {
            // Arrange
            var apoliceId = Guid.NewGuid();
            var apolice = CriarApoliceAtiva(apoliceId);
            apolice.Suspender(); // torna inativa → domínio lançará RegraNegocioException
            _apoliceRepository.ObterPorIdAsync(apoliceId).Returns(apolice);

            // Act
            var ato = () => _handler.Handle(ComandoValido(apoliceId), CancellationToken.None);

            // Assert
            await ato.Should().ThrowAsync<RegraNegocioException>();
            await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
        }

        // ─── Handler repassa RegraNegocioException do repositório ──────────────────

        [Fact]
        public async Task Handle_RepositorioLancaExcecao_NaoChamaCommitAsync()
        {
            // Arrange
            var apoliceId = Guid.NewGuid();
            var apolice = CriarApoliceAtiva(apoliceId);
            _apoliceRepository.ObterPorIdAsync(apoliceId).Returns(apolice);

            // Simula falha no repositório de sinistros
            _sinistroRepository.AdicionarAsync(Arg.Any<Sinistro>())
                .Throws(new InvalidOperationException("Falha de banco simulada."));

            // Act
            var ato = () => _handler.Handle(ComandoValido(apoliceId), CancellationToken.None);

            // Assert
            await ato.Should().ThrowAsync<InvalidOperationException>();
            await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
        }
    }

    public class AtualizarStatusHandlerApplicationTests
    {
        private readonly ISinistroRepository _sinistroRepository = Substitute.For<ISinistroRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly AtualizarStatusCommandHandler _handler;

        public AtualizarStatusHandlerApplicationTests()
        {
            _handler = new AtualizarStatusCommandHandler(_sinistroRepository, _unitOfWork);
        }

        // ─── Helpers ────────────────────────────────────────────────────────────────

        private static Sinistro CriarSinistroAberto()
        {
            var s = Sinistro.Abrir(
                Guid.NewGuid(),
                DateTime.UtcNow.AddDays(-1),
                "Colisão traseira em via expressa.",
                new Dinheiro(12000m),
                "operador");
            return s;
        }

        // ─── NotFoundException quando sinistro não existe ───────────────────────────

        [Fact]
        public async Task Handle_SinistroNaoExiste_LancaNotFoundException()
        {
            // Arrange
            var id = Guid.NewGuid();
            _sinistroRepository.ObterPorIdAsync(id).Returns((Sinistro)null!);

            var command = new AtualizarStatusCommand(id, "EmAnalise", "op");

            // Act
            var ato = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            await ato.Should().ThrowAsync<NotFoundException>()
                .WithMessage("*Sinistro*");

            await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
        }

        // ─── RegraNegocioException propagada do agregado ────────────────────────────

        [Fact]
        public async Task Handle_TransicaoInvalida_PropagaRegraNegocioExceptionDoAgregado()
        {
            // Arrange — sinistro Aberto, tentar aprovar direto (inválido pela máquina de estados)
            var id = Guid.NewGuid();
            var sinistro = CriarSinistroAberto();
            ReflectionHelper.SetPrivateId(sinistro, id);
            _sinistroRepository.ObterPorIdAsync(id).Returns(sinistro);

            var command = new AtualizarStatusCommand(id, "Aprovado", "op");

            // Act
            var ato = () => _handler.Handle(command, CancellationToken.None);

            // Assert — a exceção vem do agregado, o handler não a engole
            await ato.Should().ThrowAsync<RegraNegocioException>()
                .WithMessage("*Transição inválida*");

            await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
        }

        // ─── Negar sem motivo: MotivoNegativa VO lança, handler propaga ─────────────

        [Fact]
        public async Task Handle_NegarSemMotivo_PropagaRegraNegocioExceptionDoVO()
        {
            // Arrange
            var id = Guid.NewGuid();
            var sinistro = CriarSinistroAberto();
            ReflectionHelper.SetPrivateId(sinistro, id);
            _sinistroRepository.ObterPorIdAsync(id).Returns(sinistro);

            var command = new AtualizarStatusCommand(id, "Negado", "op", MotivoNegativa: null);

            // Act
            var ato = () => _handler.Handle(command, CancellationToken.None);

            // Assert — quem reclama é o VO MotivoNegativa, não o handler
            await ato.Should().ThrowAsync<RegraNegocioException>()
                .WithMessage("*motivo*");

            await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
        }

        // ─── CommitAsync chamado 1 vez no caminho feliz ─────────────────────────────

        [Fact]
        public async Task Handle_CaminhoFelizEmAnalise_ChamaCommitAsync1Vez()
        {
            // Arrange
            var id = Guid.NewGuid();
            var sinistro = CriarSinistroAberto();
            ReflectionHelper.SetPrivateId(sinistro, id);
            _sinistroRepository.ObterPorIdAsync(id).Returns(sinistro);

            var command = new AtualizarStatusCommand(id, "EmAnalise", "op");

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
        }
    }

    // ─── Utilidade compartilhada para setar Id privado em AggregateRoot ──────────

    internal static class ReflectionHelper
    {
        internal static void SetPrivateId(object obj, Guid id)
        {
            var type = obj.GetType();
            while (type != null)
            {
                var field = type.GetField("Id",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (field != null) { field.SetValue(obj, id); return; }

                var prop = type.GetProperty("Id",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (prop != null && prop.CanWrite) { prop.SetValue(obj, id); return; }

                type = type.BaseType;
            }
        }
    }
}
