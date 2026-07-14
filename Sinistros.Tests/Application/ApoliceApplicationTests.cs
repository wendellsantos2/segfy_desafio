using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Sinistros.Application.Apolices.Commands;
using Sinistros.Application.Apolices.Queries;
using Sinistros.Application.DTOs;
using Sinistros.Domain.Apolices;
using Sinistros.Domain.Enums;
using Sinistros.Domain.Exceptions;
using Sinistros.Domain.SeedWork;
using Xunit;

namespace Sinistros.Tests.Application
{
    public class ApoliceApplicationTests
    {
        private readonly IApoliceRepository _apoliceRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ApoliceApplicationTests()
        {
            _apoliceRepository = Substitute.For<IApoliceRepository>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
        }

        [Fact]
        public async Task ObterApolicePorId_DeveRetornarResponse_QuandoExiste()
        {
            // Arrange
            var id = Guid.NewGuid();
            var apolice = Apolice.Emitir(new NumeroApolice("ABCD-123456"), Guid.NewGuid(), Ramo.Auto, new PeriodoVigencia(DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow.AddMonths(1)));
            SetPrivateField(apolice, "Id", id);

            _apoliceRepository.ObterPorIdAsync(id, Arg.Any<CancellationToken>()).Returns(apolice);

            var query = new ObterApolicePorIdQuery(id);
            var handler = new ObterApolicePorIdQueryHandler(_apoliceRepository);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
            Assert.Equal("ABCD-123456", result.Numero);
        }

        [Fact]
        public async Task ObterApolicePorId_DeveLancarNotFoundException_QuandoNaoExiste()
        {
            // Arrange
            var id = Guid.NewGuid();
            _apoliceRepository.ObterPorIdAsync(id, Arg.Any<CancellationToken>()).Returns((Apolice)null!);

            var query = new ObterApolicePorIdQuery(id);
            var handler = new ObterApolicePorIdQueryHandler(_apoliceRepository);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(query, CancellationToken.None));
        }

        [Fact]
        public async Task ListarApolices_DeveRetornarListaPaginada()
        {
            // Arrange
            var items = new List<Apolice>
            {
                Apolice.Emitir(new NumeroApolice("AAAA-111111"), Guid.NewGuid(), Ramo.Auto, new PeriodoVigencia(DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow.AddMonths(1)))
            };

            _apoliceRepository.ListarAsync("Ativa", 1, 10, Arg.Any<CancellationToken>())
                .Returns((items, 1));

            var query = new ListarApolicesQuery("Ativa", 1, 10);
            var handler = new ListarApolicesQueryHandler(_apoliceRepository);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Items);
            Assert.Equal(1, result.TotalItems);
            Assert.Equal("AAAA-111111", result.Items.First().Numero);
        }

        [Fact]
        public async Task AtualizarStatusApolice_DeveChamarSuspender_QuandoRequisitado()
        {
            // Arrange
            var id = Guid.NewGuid();
            var apolice = Apolice.Emitir(new NumeroApolice("ABCD-123456"), Guid.NewGuid(), Ramo.Auto, new PeriodoVigencia(DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow.AddMonths(1)));
            SetPrivateField(apolice, "Id", id);

            _apoliceRepository.ObterPorIdAsync(id, Arg.Any<CancellationToken>()).Returns(apolice);

            var command = new AtualizarStatusApoliceCommand(id, "Suspensa");
            var handler = new AtualizarStatusApoliceCommandHandler(_apoliceRepository, _unitOfWork);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(StatusApolice.Suspensa, apolice.Status);
            await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task AtualizarStatusApolice_DeveLancarRegraNegocioException_QuandoTransicaoInvalida()
        {
            // Arrange
            var id = Guid.NewGuid();
            var apolice = Apolice.Emitir(new NumeroApolice("ABCD-123456"), Guid.NewGuid(), Ramo.Auto, new PeriodoVigencia(DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow.AddMonths(1)));
            SetPrivateField(apolice, "Id", id);
            apolice.Cancelar(); // Cancelada permanentemente

            _apoliceRepository.ObterPorIdAsync(id, Arg.Any<CancellationToken>()).Returns(apolice);

            var command = new AtualizarStatusApoliceCommand(id, "Suspensa"); // Tentar suspender a partir de Cancelada (Inválido!)
            var handler = new AtualizarStatusApoliceCommandHandler(_apoliceRepository, _unitOfWork);

            // Act & Assert
            await Assert.ThrowsAsync<RegraNegocioException>(() => handler.Handle(command, CancellationToken.None));
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
