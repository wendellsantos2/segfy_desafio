using System;
using System.Linq;
using FluentAssertions;
using Sinistros.Domain.Enums;
using Sinistros.Domain.Exceptions;
using Sinistros.Domain.SeedWork;
using Sinistros.Domain.Sinistros;
using Sinistros.Domain.Sinistros.Events;
using Xunit;

namespace Sinistros.Tests.Domain
{
    /// <summary>
    /// Passo 25 - Invariantes, geração de histórico e Domain Events.
    /// Prova que cada transição gera exatamente 1 entrada de histórico com StatusAnterior correto
    /// e 1 Domain Event do tipo correspondente na AggregateRoot.
    /// </summary>
    public class InvarianteSinistroTests
    {
        // ─── Helpers ────────────────────────────────────────────────────────────────

        private static Sinistro CriarSinistroAberto() =>
            Sinistro.Abrir(
                Guid.NewGuid(),
                DateTime.UtcNow.AddDays(-1),
                "Dano estrutural por granizo no telhado.",
                new Dinheiro(20000m),
                "operador");

        private static readonly MotivoNegativa MotivoValido =
            new("Sinistro fora do escopo da apólice de saúde.");

        // ─── Encerrar com valor zero ou nulo ────────────────────────────────────────

        [Fact]
        public void Encerrar_ComValorZero_LancaRegraNegocioException()
        {
            // Arrange
            var sinistro = CriarSinistroAberto();
            sinistro.EnviarParaAnalise("op");
            sinistro.Aprovar("op");

            // Act
            var ato = () => sinistro.Encerrar(new Dinheiro(0m), "op");

            // Assert
            ato.Should().Throw<RegraNegocioException>()
               .WithMessage("*maior que zero*");
        }

        // ─── Encerrar preenche DataEncerramento ──────────────────────────────────────

        [Fact]
        public void Encerrar_CaminhoFeliz_PreencheDataEncerramento()
        {
            // Arrange
            var sinistro = CriarSinistroAberto();
            sinistro.EnviarParaAnalise("op");
            sinistro.Aprovar("op");
            var antes = DateTime.UtcNow;

            // Act
            sinistro.Encerrar(new Dinheiro(15000m), "op");

            // Assert
            sinistro.DataEncerramento.Should().NotBeNull();
            sinistro.DataEncerramento!.Value.Should().BeOnOrAfter(antes);
        }

        // ─── Negar preenche o VO MotivoNegativa ──────────────────────────────────────

        [Fact]
        public void Negar_CaminhoFeliz_PreencheMotivoNegativa()
        {
            // Arrange
            var sinistro = CriarSinistroAberto();
            var motivo = new MotivoNegativa("Cobertura expirada antes da ocorrência.");

            // Act
            sinistro.Negar(motivo, "analista");

            // Assert
            sinistro.Motivo.Should().NotBeNull();
            sinistro.Motivo!.Texto.Should().Be("Cobertura expirada antes da ocorrência.");
        }

        // ─── Cada transição incrementa o histórico em 1 ──────────────────────────────

        [Fact]
        public void EnviarParaAnalise_AdicionaExatamente1RegistroDeHistorico()
        {
            // Arrange
            var sinistro = CriarSinistroAberto(); // já tem 1 (abertura)

            // Act
            sinistro.EnviarParaAnalise("op");

            // Assert
            sinistro.HistoricoSinistros.Should().HaveCount(2);
        }

        [Fact]
        public void EnviarParaAnalise_StatusAnteriorNoHistoricoEAberto()
        {
            // Arrange
            var sinistro = CriarSinistroAberto();

            // Act
            sinistro.EnviarParaAnalise("op");

            // Assert — o registro de transição (índice 1) deve ter StatusAnterior = Aberto
            var transicao = sinistro.HistoricoSinistros
                .Skip(1).First(); // pula o de abertura
            transicao.StatusAnterior.Should().Be(StatusSinistro.Aberto);
            transicao.StatusNovo.Should().Be(StatusSinistro.EmAnalise);
        }

        [Fact]
        public void Negar_APartirDeAberto_StatusAnteriorNoHistoricoEAberto()
        {
            // Arrange
            var sinistro = CriarSinistroAberto();

            // Act
            sinistro.Negar(MotivoValido, "op");

            // Assert
            var transicao = sinistro.HistoricoSinistros.Skip(1).First();
            transicao.StatusAnterior.Should().Be(StatusSinistro.Aberto);
            transicao.StatusNovo.Should().Be(StatusSinistro.Negado);
        }

        // ─── Cada transição adiciona o Domain Event correto ──────────────────────────

        [Fact]
        public void EnviarParaAnalise_AdicionaSinistroEnviadoParaAnaliseEvent()
        {
            // Arrange
            var sinistro = CriarSinistroAberto();
            sinistro.LimparEventos(); // zera o SinistroAbertoEvent

            // Act
            sinistro.EnviarParaAnalise("op");

            // Assert
            sinistro.DomainEvents.Should().ContainSingle(e => e is SinistroEnviadoParaAnaliseEvent);
        }

        [Fact]
        public void Aprovar_AdicionaSinistroAprovadoEvent()
        {
            // Arrange
            var sinistro = CriarSinistroAberto();
            sinistro.EnviarParaAnalise("op");
            sinistro.LimparEventos();

            // Act
            sinistro.Aprovar("op");

            // Assert
            sinistro.DomainEvents.Should().ContainSingle(e => e is SinistroAprovadoEvent);
        }

        [Fact]
        public void Negar_AdicionaSinistroNegadoEvent()
        {
            // Arrange
            var sinistro = CriarSinistroAberto();
            sinistro.LimparEventos();

            // Act
            sinistro.Negar(MotivoValido, "op");

            // Assert
            sinistro.DomainEvents.Should().ContainSingle(e => e is SinistroNegadoEvent);
        }

        [Fact]
        public void Encerrar_AdicionaSinistroEncerradoEvent()
        {
            // Arrange
            var sinistro = CriarSinistroAberto();
            sinistro.EnviarParaAnalise("op");
            sinistro.Aprovar("op");
            sinistro.LimparEventos();

            // Act
            sinistro.Encerrar(new Dinheiro(18000m), "op");

            // Assert
            sinistro.DomainEvents.Should().ContainSingle(e => e is SinistroEncerradoEvent);
        }

        // ─── Percurso completo: 4 registros de histórico ─────────────────────────────

        [Fact]
        public void PercursoCompleto_AbertoEmAnaliseAprovadoEncerrado_Gera4RegistrosDeHistorico()
        {
            // Arrange
            var sinistro = CriarSinistroAberto(); // histórico[0]: Abertura

            // Act
            sinistro.EnviarParaAnalise("op"); // histórico[1]
            sinistro.Aprovar("op");           // histórico[2]
            sinistro.Encerrar(new Dinheiro(19000m), "op"); // histórico[3]

            // Assert
            sinistro.HistoricoSinistros.Should().HaveCount(4);

            var lista = sinistro.HistoricoSinistros.ToList();
            lista[0].StatusAnterior.Should().BeNull();
            lista[0].StatusNovo.Should().Be(StatusSinistro.Aberto);

            lista[1].StatusAnterior.Should().Be(StatusSinistro.Aberto);
            lista[1].StatusNovo.Should().Be(StatusSinistro.EmAnalise);

            lista[2].StatusAnterior.Should().Be(StatusSinistro.EmAnalise);
            lista[2].StatusNovo.Should().Be(StatusSinistro.Aprovado);

            lista[3].StatusAnterior.Should().Be(StatusSinistro.Aprovado);
            lista[3].StatusNovo.Should().Be(StatusSinistro.Encerrado);
        }
    }
}
