using System;
using FluentAssertions;
using Sinistros.Domain.Apolices;
using Sinistros.Domain.Enums;
using Sinistros.Domain.Exceptions;
using Sinistros.Domain.SeedWork;
using Sinistros.Domain.Sinistros;
using Xunit;

namespace Sinistros.Tests.Domain
{
    /// <summary>
    /// Passo 24 - Matriz completa de transições de status do sinistro.
    /// Usa [Theory] + [InlineData] para cobrir todas as combinações válidas e inválidas.
    /// </summary>
    public class TransicaoStatusSinistroTests
    {
        // ─── Helpers ────────────────────────────────────────────────────────────────

        private static Sinistro CriarSinistroAberto() =>
            Sinistro.Abrir(
                Guid.NewGuid(),
                DateTime.UtcNow.AddDays(-1),
                "Batida traseira na via expressa.",
                new Dinheiro(10000m),
                "operador");

        private static Sinistro EmAnalise()
        {
            var s = CriarSinistroAberto();
            s.EnviarParaAnalise("operador");
            return s;
        }

        private static Sinistro Aprovado()
        {
            var s = EmAnalise();
            s.Aprovar("operador");
            return s;
        }

        // ─── Transições VÁLIDAS ──────────────────────────────────────────────────────

        [Theory]
        [InlineData("Aberto -> EmAnalise")]
        [InlineData("Aberto -> Negado")]
        [InlineData("EmAnalise -> Aprovado")]
        [InlineData("EmAnalise -> Negado")]
        [InlineData("Aprovado -> Encerrado")]
        public void AplicarTransicao_TransicaoValida_NaoLancaExcecao(string transicao)
        {
            Action ato = transicao switch
            {
                "Aberto -> EmAnalise" => () => CriarSinistroAberto().EnviarParaAnalise("op"),
                "Aberto -> Negado" => () => CriarSinistroAberto().Negar(new MotivoNegativa("Motivo válido com dez chars."), "op"),
                "EmAnalise -> Aprovado" => () => EmAnalise().Aprovar("op"),
                "EmAnalise -> Negado" => () => EmAnalise().Negar(new MotivoNegativa("Motivo válido com dez chars."), "op"),
                "Aprovado -> Encerrado" => () => Aprovado().Encerrar(new Dinheiro(9000m), "op"),
                _ => throw new ArgumentException($"Transição desconhecida: {transicao}")
            };

            ato.Should().NotThrow();
        }

        // ─── Transições INVÁLIDAS ────────────────────────────────────────────────────

        [Theory]
        [InlineData("Aberto -> Aprovado")]
        [InlineData("Aberto -> Encerrado")]
        [InlineData("EmAnalise -> Encerrado")]
        [InlineData("Aprovado -> Negado")]
        [InlineData("Aprovado -> EmAnalise")]
        [InlineData("Negado -> EmAnalise")]
        [InlineData("Negado -> Aprovado")]
        [InlineData("Negado -> Encerrado")]
        [InlineData("Encerrado -> EmAnalise")]
        [InlineData("Encerrado -> Aprovado")]
        [InlineData("Encerrado -> Negado")]
        public void AplicarTransicao_TransicaoInvalida_LancaRegraNegocioException(string transicao)
        {
            // Arrange
            var negado = CriarSinistroAberto();
            negado.Negar(new MotivoNegativa("Motivo válido com dez chars."), "op");

            var encerrado = Aprovado();
            encerrado.Encerrar(new Dinheiro(8000m), "op");

            Action ato = transicao switch
            {
                "Aberto -> Aprovado" => () => CriarSinistroAberto().Aprovar("op"),
                "Aberto -> Encerrado" => () => CriarSinistroAberto().Encerrar(new Dinheiro(8000m), "op"),
                "EmAnalise -> Encerrado" => () => EmAnalise().Encerrar(new Dinheiro(8000m), "op"),
                "Aprovado -> Negado" => () => Aprovado().Negar(new MotivoNegativa("Motivo válido com dez chars."), "op"),
                "Aprovado -> EmAnalise" => () => Aprovado().EnviarParaAnalise("op"),
                "Negado -> EmAnalise" => () => negado.EnviarParaAnalise("op"),
                "Negado -> Aprovado" => () => negado.Aprovar("op"),
                "Negado -> Encerrado" => () => negado.Encerrar(new Dinheiro(8000m), "op"),
                "Encerrado -> EmAnalise" => () => encerrado.EnviarParaAnalise("op"),
                "Encerrado -> Aprovado" => () => encerrado.Aprovar("op"),
                "Encerrado -> Negado" => () => encerrado.Negar(new MotivoNegativa("Motivo válido com dez chars."), "op"),
                _ => throw new ArgumentException($"Transição desconhecida: {transicao}")
            };

            // Act & Assert
            ato.Should().Throw<RegraNegocioException>()
               .WithMessage("*Transição inválida*");
        }

        // ─── Apólice cancelada não reativa ───────────────────────────────────────────

        [Fact]
        public void Reativar_ApoliceCancelada_LancaRegraNegocioException()
        {
            // Arrange
            var apolice = Apolice.Emitir(
                new NumeroApolice("ABCD-123456"),
                Guid.NewGuid(),
                Ramo.Vida,
                new PeriodoVigencia(DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow.AddMonths(11)));
            apolice.Cancelar();

            // Act
            var ato = () => apolice.Reativar();

            // Assert
            ato.Should().Throw<RegraNegocioException>()
               .WithMessage("*cancelada*");
        }

        [Fact]
        public void Suspender_ApoliceCancelada_LancaRegraNegocioException()
        {
            // Arrange
            var apolice = Apolice.Emitir(
                new NumeroApolice("EFGH-789012"),
                Guid.NewGuid(),
                Ramo.Residencial,
                new PeriodoVigencia(DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow.AddMonths(11)));
            apolice.Cancelar();

            // Act
            var ato = () => apolice.Suspender();

            // Assert
            ato.Should().Throw<RegraNegocioException>()
               .WithMessage("*cancelada*");
        }
    }
}
