using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using NSubstitute;
using Sinistros.Application.Behaviors;
using Sinistros.Application.Sinistros.Commands;
using Sinistros.Application.Apolices.Commands;
using Sinistros.Application.Exceptions;
using Xunit;
using ValidationException = Sinistros.Application.Exceptions.ValidationException;

namespace Sinistros.Tests.Application
{
    public class ValidationBehaviorTests
    {
        [Fact]
        public async Task ValidationBehavior_DeveLancarValidationException_QuandoAbrirSinistroCommandForInvalido()
        {
            // Arrange
            var validators = new List<IValidator<AbrirSinistroCommand>>
            {
                new AbrirSinistroCommandValidator()
            };
            var behavior = new ValidationBehavior<AbrirSinistroCommand, Guid>(validators);

            var command = new AbrirSinistroCommand(
                ApoliceId: Guid.Empty, // Inválido
                DataOcorrencia: DateTime.UtcNow.AddDays(5), // Futura (Inválido)
                Descricao: "", // Vazia (Inválido)
                ValorEstimado: -10.00m // Negativo (Inválido)
            );

            var nextDelegate = Substitute.For<RequestHandlerDelegate<Guid>>();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() =>
                behavior.Handle(command, nextDelegate, CancellationToken.None));

            Assert.NotNull(exception.Errors);
            Assert.Contains("ApoliceId", exception.Errors.Keys);
            Assert.Contains("DataOcorrencia", exception.Errors.Keys);
            Assert.Contains("Descricao", exception.Errors.Keys);
            Assert.Contains("ValorEstimado", exception.Errors.Keys);

            await nextDelegate.DidNotReceive().Invoke();
        }

        [Fact]
        public async Task ValidationBehavior_DeveLancarValidationException_QuandoCriarApoliceCommandPossuiVigenciaOuNumeroInvalido()
        {
            // Arrange
            var validators = new List<IValidator<CriarApoliceCommand>>
            {
                new CriarApoliceCommandValidator()
            };
            var behavior = new ValidationBehavior<CriarApoliceCommand, Guid>(validators);

            var command = new CriarApoliceCommand(
                Numero: "123-ABC", // Formato inválido (deve ser AAAA-NNNNNN)
                ClienteId: Guid.Empty, // Inválido
                Ramo: "RamoInvalido", // Ramo inválido
                Inicio: DateTime.UtcNow,
                Fim: DateTime.UtcNow.AddDays(-1) // Fim antes de início (Inválido)
            );

            var nextDelegate = Substitute.For<RequestHandlerDelegate<Guid>>();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() =>
                behavior.Handle(command, nextDelegate, CancellationToken.None));

            Assert.Contains("Numero", exception.Errors.Keys);
            Assert.Contains("ClienteId", exception.Errors.Keys);
            Assert.Contains("Ramo", exception.Errors.Keys);
            Assert.Contains("Fim", exception.Errors.Keys);

            await nextDelegate.DidNotReceive().Invoke();
        }

        [Fact]
        public async Task ValidationBehavior_DeveLancarValidationException_QuandoAtualizarStatusCommandNegadoNaoPossuiMotivoAdequado()
        {
            // Arrange
            var validators = new List<IValidator<AtualizarStatusCommand>>
            {
                new AtualizarStatusCommandValidator()
            };
            var behavior = new ValidationBehavior<AtualizarStatusCommand, Unit>(validators);

            var command = new AtualizarStatusCommand(
                Id: Guid.NewGuid(),
                Status: "Negado",
                Usuario: "Analista.Teste",
                MotivoNegativa: "Curto" // Menor que 10 caracteres (Inválido)
            );

            var nextDelegate = Substitute.For<RequestHandlerDelegate<Unit>>();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() =>
                behavior.Handle(command, nextDelegate, CancellationToken.None));

            Assert.Contains("MotivoNegativa", exception.Errors.Keys);
            await nextDelegate.DidNotReceive().Invoke();
        }

        [Fact]
        public async Task ValidationBehavior_DevePermitirExecucao_QuandoComandoForValido()
        {
            // Arrange
            var validators = new List<IValidator<AbrirSinistroCommand>>
            {
                new AbrirSinistroCommandValidator()
            };
            var behavior = new ValidationBehavior<AbrirSinistroCommand, Guid>(validators);

            var command = new AbrirSinistroCommand(
                ApoliceId: Guid.NewGuid(),
                DataOcorrencia: DateTime.UtcNow.AddDays(-1),
                Descricao: "Descrição válida e longa de teste.",
                ValorEstimado: 2500m
            );

            var nextDelegate = Substitute.For<RequestHandlerDelegate<Guid>>();
            nextDelegate.Invoke().Returns(Task.FromResult(Guid.NewGuid()));

            // Act
            var result = await behavior.Handle(command, nextDelegate, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, result);
            await nextDelegate.Received(1).Invoke();
        }
    }
}
