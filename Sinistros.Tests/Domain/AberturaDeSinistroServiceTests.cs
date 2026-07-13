using System;
using FluentAssertions;
using Sinistros.Domain.Apolices;
using Sinistros.Domain.Enums;
using Sinistros.Domain.Exceptions;
using Sinistros.Domain.SeedWork;
using Sinistros.Domain.Servicos;
using Sinistros.Domain.Sinistros;
using Sinistros.Domain.Sinistros.Events;
using Xunit;

namespace Sinistros.Tests.Domain
{
    /// <summary>
    /// Passo 23 - Testes de Abertura de Sinistro e Domain Service.
    /// Zero mocks: tudo é domínio puro instanciado diretamente.
    /// </summary>
    public class AberturaDeSinistroServiceTests
    {
        private readonly AberturaDeSinistroService _service = new();

        // ─── Helpers ────────────────────────────────────────────────────────────────

        private static Apolice CriarApoliceAtiva() =>
            Apolice.Emitir(
                new NumeroApolice("ABCD-123456"),
                Guid.NewGuid(),
                Ramo.Auto,
                new PeriodoVigencia(DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow.AddMonths(11)));

        // ─── Abertura com apólice Ativa ─────────────────────────────────────────────

        [Fact]
        public void Abrir_ApoliceAtiva_CriaSinistroComSucesso()
        {
            // Arrange
            var apolice = CriarApoliceAtiva();
            var dataOcorrencia = DateTime.UtcNow.AddDays(-1);

            // Act
            var sinistro = _service.Abrir(apolice, dataOcorrencia, "Colisão frontal na rodovia.", new Dinheiro(8000m));

            // Assert
            sinistro.Should().NotBeNull();
            sinistro.ApoliceId.Should().Be(apolice.Id);
            sinistro.Status.Should().Be(StatusSinistro.Aberto);
        }

        // ─── Abertura com apólice Suspensa ──────────────────────────────────────────

        [Fact]
        public void Abrir_ApoliceSuspensa_LancaRegraNegocioException()
        {
            // Arrange
            var apolice = CriarApoliceAtiva();
            apolice.Suspender();

            // Act
            var ato = () => _service.Abrir(apolice, DateTime.UtcNow.AddDays(-1), "Descrição válida aqui.", new Dinheiro(1000m));

            // Assert
            ato.Should().Throw<RegraNegocioException>()
               .WithMessage("*apolice ativa*");
        }

        // ─── Abertura com apólice Cancelada ─────────────────────────────────────────

        [Fact]
        public void Abrir_ApoliceCancelada_LancaRegraNegocioException()
        {
            // Arrange
            var apolice = CriarApoliceAtiva();
            apolice.Cancelar();

            // Act
            var ato = () => _service.Abrir(apolice, DateTime.UtcNow.AddDays(-1), "Descrição válida aqui.", new Dinheiro(1000m));

            // Assert
            ato.Should().Throw<RegraNegocioException>()
               .WithMessage("*apolice ativa*");
        }

        // ─── Sinistro recém-aberto: estado esperado ──────────────────────────────────

        [Fact]
        public void Abrir_ApoliceAtiva_SinistroTemStatusAberto()
        {
            // Arrange
            var apolice = CriarApoliceAtiva();

            // Act
            var sinistro = _service.Abrir(apolice, DateTime.UtcNow.AddDays(-1), "Incêndio na cozinha do imóvel.", new Dinheiro(15000m), "operador1");

            // Assert
            sinistro.Status.Should().Be(StatusSinistro.Aberto);
        }

        [Fact]
        public void Abrir_ApoliceAtiva_SinistroTemExatamente1HistoricoComStatusAnteriorNull()
        {
            // Arrange
            var apolice = CriarApoliceAtiva();

            // Act
            var sinistro = _service.Abrir(apolice, DateTime.UtcNow.AddDays(-1), "Furto de veículo na garagem.", new Dinheiro(30000m), "operador1");

            // Assert
            sinistro.HistoricoSinistros.Should().HaveCount(1);
            sinistro.HistoricoSinistros.Should().ContainSingle(h =>
                h.StatusAnterior == null && h.StatusNovo == StatusSinistro.Aberto);
        }

        [Fact]
        public void Abrir_ApoliceAtiva_SinistroTem1SinistroAbertoEventNaColecao()
        {
            // Arrange
            var apolice = CriarApoliceAtiva();

            // Act
            var sinistro = _service.Abrir(apolice, DateTime.UtcNow.AddDays(-1), "Alagamento por chuva intensa.", new Dinheiro(5000m), "operador1");

            // Assert
            sinistro.DomainEvents.Should().HaveCount(1);
            sinistro.DomainEvents.Should().ContainSingle(e => e is SinistroAbertoEvent);
        }

        // ─── Data de ocorrência futura ───────────────────────────────────────────────

        [Fact]
        public void Abrir_DataOcorrenciaFutura_LancaRegraNegocioException()
        {
            // Arrange
            var apolice = CriarApoliceAtiva();
            var dataFutura = DateTime.UtcNow.AddDays(1);

            // Act
            var ato = () => _service.Abrir(apolice, dataFutura, "Tentativa com data futura.", new Dinheiro(1000m));

            // Assert
            ato.Should().Throw<RegraNegocioException>()
               .WithMessage("*futuro*");
        }
    }
}
