using System;
using FluentAssertions;
using Sinistros.Domain.Apolices;
using Sinistros.Domain.Clientes;
using Sinistros.Domain.Exceptions;
using Sinistros.Domain.SeedWork;
using Sinistros.Domain.Sinistros;
using Xunit;

namespace Sinistros.Tests.Domain
{
    /// <summary>
    /// Passo 22 - Testes de Value Objects
    /// Padrão: Metodo_Cenario_ResultadoEsperado | Arrange / Act / Assert
    /// </summary>
    public class ValueObjectTests
    {
        // ─── Dinheiro ───────────────────────────────────────────────────────────────

        [Fact]
        public void Dinheiro_ComValorNegativo_LancaRegraNegocioException()
        {
            // Arrange & Act
            var ato = () => new Dinheiro(-0.01m);

            // Assert
            ato.Should().Throw<RegraNegocioException>()
               .WithMessage("*negativo*");
        }

        [Fact]
        public void Dinheiro_ComZero_NaoLancaExcecao()
        {
            // Arrange & Act
            var ato = () => new Dinheiro(0m);

            // Assert
            ato.Should().NotThrow();
        }

        [Fact]
        public void Dinheiro_DoisObjetosComMesmoValorEMoeda_SaoIguais()
        {
            // Arrange
            var d1 = new Dinheiro(1500.50m, "BRL");
            var d2 = new Dinheiro(1500.50m, "BRL");

            // Act & Assert
            d1.Should().Be(d2);
            d1.Equals(d2).Should().BeTrue();
            (d1 == d2).Should().BeTrue();
        }

        [Fact]
        public void Dinheiro_ObjetosComValoresDiferentes_NaoSaoIguais()
        {
            // Arrange
            var d1 = new Dinheiro(100m);
            var d2 = new Dinheiro(200m);

            // Act & Assert
            d1.Should().NotBe(d2);
        }

        [Fact]
        public void Dinheiro_ObjetosComMoedasDiferentes_NaoSaoIguais()
        {
            // Arrange
            var d1 = new Dinheiro(100m, "BRL");
            var d2 = new Dinheiro(100m, "USD");

            // Act & Assert
            d1.Should().NotBe(d2);
        }

        [Fact]
        public void Dinheiro_ComMoedaVazia_LancaRegraNegocioException()
        {
            // Arrange & Act
            var ato = () => new Dinheiro(100m, "");

            // Assert
            ato.Should().Throw<RegraNegocioException>()
               .WithMessage("*moeda*");
        }

        // ─── MotivoNegativa ─────────────────────────────────────────────────────────

        [Fact]
        public void MotivoNegativa_ComTextoVazio_LancaRegraNegocioException()
        {
            // Arrange & Act
            var ato = () => new MotivoNegativa("");

            // Assert
            ato.Should().Throw<RegraNegocioException>()
               .WithMessage("*vazio*");
        }

        [Fact]
        public void MotivoNegativa_ComTextoMenosDe10Chars_LancaRegraNegocioException()
        {
            // Arrange & Act
            var ato = () => new MotivoNegativa("curto");   // 5 chars

            // Assert
            ato.Should().Throw<RegraNegocioException>()
               .WithMessage("*10 caracteres*");
        }

        [Fact]
        public void MotivoNegativa_ComTextoValido_CriaComSucesso()
        {
            // Arrange & Act
            var motivo = new MotivoNegativa("Sinistro fora do escopo da apólice contratada.");

            // Assert
            motivo.Texto.Should().Be("Sinistro fora do escopo da apólice contratada.");
        }

        [Fact]
        public void MotivoNegativa_ComExatamente10Chars_NaoLancaExcecao()
        {
            // Arrange & Act
            var ato = () => new MotivoNegativa("1234567890"); // exatamente 10

            // Assert
            ato.Should().NotThrow();
        }

        // ─── PeriodoVigencia ────────────────────────────────────────────────────────

        [Fact]
        public void PeriodoVigencia_ComFimAntesDoInicio_LancaRegraNegocioException()
        {
            // Arrange
            var inicio = new DateTime(2025, 1, 1);
            var fim = new DateTime(2024, 12, 31);

            // Act
            var ato = () => new PeriodoVigencia(inicio, fim);

            // Assert
            ato.Should().Throw<RegraNegocioException>()
               .WithMessage("*término*maior*");
        }

        [Fact]
        public void PeriodoVigencia_ComFimIgualAoInicio_LancaRegraNegocioException()
        {
            // Arrange
            var data = new DateTime(2025, 6, 1);

            // Act
            var ato = () => new PeriodoVigencia(data, data);

            // Assert
            ato.Should().Throw<RegraNegocioException>();
        }

        [Fact]
        public void EstaVigenteEm_NoBordaDoInicio_RetornaTrue()
        {
            // Arrange
            var inicio = new DateTime(2025, 1, 1);
            var fim = new DateTime(2025, 12, 31);
            var vigencia = new PeriodoVigencia(inicio, fim);

            // Act & Assert
            vigencia.EstaVigenteEm(inicio).Should().BeTrue();
        }

        [Fact]
        public void EstaVigenteEm_NaBordaDoFim_RetornaTrue()
        {
            // Arrange
            var inicio = new DateTime(2025, 1, 1);
            var fim = new DateTime(2025, 12, 31);
            var vigencia = new PeriodoVigencia(inicio, fim);

            // Act & Assert
            vigencia.EstaVigenteEm(fim).Should().BeTrue();
        }

        [Fact]
        public void EstaVigenteEm_ForaDoIntervalo_RetornaFalse()
        {
            // Arrange
            var vigencia = new PeriodoVigencia(new DateTime(2025, 1, 1), new DateTime(2025, 12, 31));

            // Act & Assert
            vigencia.EstaVigenteEm(new DateTime(2026, 1, 1)).Should().BeFalse();
            vigencia.EstaVigenteEm(new DateTime(2024, 12, 31)).Should().BeFalse();
        }

        // ─── Documento ──────────────────────────────────────────────────────────────

        [Fact]
        public void Documento_ComCpfInvalido_LancaRegraNegocioException()
        {
            // Arrange & Act
            var ato = () => new Documento("111.111.111-11"); // dígitos todos iguais = inválido

            // Assert
            ato.Should().Throw<RegraNegocioException>()
               .WithMessage("*CPF inválido*");
        }

        [Fact]
        public void Documento_ComNumerosInsuficientes_LancaRegraNegocioException()
        {
            // Arrange & Act — 10 dígitos, nem CPF nem CNPJ
            var ato = () => new Documento("1234567890");

            // Assert
            ato.Should().Throw<RegraNegocioException>()
               .WithMessage("*CPF*CNPJ*");
        }

        [Fact]
        public void Documento_ComCpfValido_CriaComSucesso()
        {
            // Arrange & Act — CPF válido conhecido: 529.982.247-25
            var doc = new Documento("529.982.247-25");

            // Assert
            doc.Numero.Should().Be("52998224725");
        }

        [Fact]
        public void Documento_Vazio_LancaRegraNegocioException()
        {
            // Arrange & Act
            var ato = () => new Documento("");

            // Assert
            ato.Should().Throw<RegraNegocioException>()
               .WithMessage("*vazio*");
        }
    }
}
