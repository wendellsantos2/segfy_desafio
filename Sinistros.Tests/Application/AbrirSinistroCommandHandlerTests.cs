using System;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Sinistros.Application.DTOs;
using Sinistros.Application.Sinistros.Commands;
using Sinistros.Domain.Apolices;
using Sinistros.Domain.Clientes;
using Sinistros.Domain.Enums;
using Sinistros.Domain.Exceptions;
using Sinistros.Domain.SeedWork;
using Sinistros.Domain.Servicos;
using Sinistros.Domain.Sinistros;
using Xunit;

namespace Sinistros.Tests.Application
{
    public class AbrirSinistroCommandHandlerTests
    {
        private readonly IApoliceRepository _apoliceRepository;
        private readonly ISinistroRepository _sinistroRepository;
        private readonly AberturaDeSinistroService _aberturaDeSinistroService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly AbrirSinistroCommandHandler _handler;

        public AbrirSinistroCommandHandlerTests()
        {
            _apoliceRepository = Substitute.For<IApoliceRepository>();
            _sinistroRepository = Substitute.For<ISinistroRepository>();
            _aberturaDeSinistroService = new AberturaDeSinistroService();
            _unitOfWork = Substitute.For<IUnitOfWork>();

            _handler = new AbrirSinistroCommandHandler(
                _apoliceRepository,
                _sinistroRepository,
                _aberturaDeSinistroService,
                _unitOfWork
            );
        }

        [Fact]
        public async Task Handle_DeveAbrirSinistroComSucesso_QuandoApoliceExisteEAtiva()
        {
            // Arrange
            var apoliceId = Guid.NewGuid();
            var clienteId = Guid.NewGuid();
            var vigencia = new PeriodoVigencia(DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow.AddMonths(1));
            var apolice = Apolice.Emitir(new NumeroApolice("ABCD-123456"), clienteId, Ramo.Auto, vigencia);
            // Garantir que a apólice tem o ID correto no mock
            SetPrivateField(apolice, "Id", apoliceId);

            _apoliceRepository.ObterPorIdAsync(apoliceId).Returns(apolice);

            var command = new AbrirSinistroCommand(
                ApoliceId: apoliceId,
                DataOcorrencia: DateTime.UtcNow.AddDays(-2),
                Descricao: "Batida traseira na rodovia federal.",
                ValorEstimado: 5000.00m
            );

            // Act
            var response = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(apoliceId, response.ApoliceId);
            Assert.Equal(command.Descricao, response.Descricao);
            Assert.Equal(command.ValorEstimado, response.ValorEstimado);
            Assert.Equal(StatusSinistro.Aberto.ToString(), response.Status);

            await _sinistroRepository.Received(1).AdicionarAsync(Arg.Any<Sinistro>());
            await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_DeveLancarNotFoundException_QuandoApoliceNaoExiste()
        {
            // Arrange
            var apoliceId = Guid.NewGuid();
            _apoliceRepository.ObterPorIdAsync(apoliceId).Returns((Apolice)null!);

            var command = new AbrirSinistroCommand(
                ApoliceId: apoliceId,
                DataOcorrencia: DateTime.UtcNow.AddDays(-2),
                Descricao: "Descrição de teste",
                ValorEstimado: 1000m
            );

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                _handler.Handle(command, CancellationToken.None));

            Assert.Equal("Apólice não encontrada.", exception.Message);
            await _sinistroRepository.DidNotReceive().AdicionarAsync(Arg.Any<Sinistro>());
            await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_DeveLancarRegraNegocioException_QuandoApoliceNaoEstaAtiva()
        {
            // Arrange
            var apoliceId = Guid.NewGuid();
            var clienteId = Guid.NewGuid();
            var vigencia = new PeriodoVigencia(DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow.AddMonths(1));
            var apolice = Apolice.Emitir(new NumeroApolice("ABCD-123456"), clienteId, Ramo.Auto, vigencia);
            SetPrivateField(apolice, "Id", apoliceId);
            
            // Suspender apólice para torná-la inativa
            apolice.Suspender();

            _apoliceRepository.ObterPorIdAsync(apoliceId).Returns(apolice);

            var command = new AbrirSinistroCommand(
                ApoliceId: apoliceId,
                DataOcorrencia: DateTime.UtcNow.AddDays(-2),
                Descricao: "Descrição de teste",
                ValorEstimado: 1000m
            );

            // Act & Assert
            var exception = await Assert.ThrowsAsync<RegraNegocioException>(() =>
                _handler.Handle(command, CancellationToken.None));

            Assert.Equal("Sinistro so pode ser aberto em apolice ativa", exception.Message);
            await _sinistroRepository.DidNotReceive().AdicionarAsync(Arg.Any<Sinistro>());
            await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
        }

        private static void SetPrivateField(object obj, string fieldName, object value)
        {
            var property = obj.GetType().GetProperty(fieldName);
            if (property != null && property.CanWrite)
            {
                property.SetValue(obj, value);
                return;
            }

            var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (field != null)
            {
                field.SetValue(obj, value);
            }
            else
            {
                var baseType = obj.GetType().BaseType;
                while (baseType != null)
                {
                    field = baseType.GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                    if (field != null)
                    {
                        field.SetValue(obj, value);
                        break;
                    }
                    baseType = baseType.BaseType;
                }
            }
        }
    }
}
