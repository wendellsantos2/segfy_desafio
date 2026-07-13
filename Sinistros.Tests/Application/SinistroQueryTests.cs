using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Sinistros.Application.DTOs;
using Sinistros.Application.Interfaces;
using Sinistros.Application.Sinistros.Queries;
using Sinistros.Domain.Exceptions;
using Xunit;

namespace Sinistros.Tests.Application
{
    public class SinistroQueryTests
    {
        private readonly ISinistroQueries _sinistroQueries;

        public SinistroQueryTests()
        {
            _sinistroQueries = Substitute.For<ISinistroQueries>();
        }

        [Fact]
        public void PagedResult_DeveCalcularCorretamenteMetadadosDePagina()
        {
            // Arrange & Act
            var items = new List<string> { "item1", "item2" };
            var result = new PagedResult<string>(items, totalItems: 25, page: 2, pageSize: 10);

            // Assert
            Assert.Equal(3, result.TotalPages); // 25 / 10 = 2.5 -> teto = 3
            Assert.True(result.HasNext); // Página 2 de 3 -> tem próxima
            Assert.True(result.HasPrevious); // Página 2 de 3 -> tem anterior
        }

        [Fact]
        public void PagedResult_DeveCalcularHasNextEHasPreviousCorretamenteNaPrimeiraDeVariasPaginas()
        {
            // Arrange & Act
            var items = new List<string> { "item1" };
            var result = new PagedResult<string>(items, totalItems: 15, page: 1, pageSize: 10);

            // Assert
            Assert.Equal(2, result.TotalPages);
            Assert.True(result.HasNext);
            Assert.False(result.HasPrevious);
        }

        [Fact]
        public async Task ObterSinistroPorId_DeveRetornarResponse_QuandoExiste()
        {
            // Arrange
            var id = Guid.NewGuid();
            var response = new SinistroResponse
            {
                Id = id,
                ApoliceId = Guid.NewGuid(),
                DataOcorrencia = DateTime.UtcNow.AddDays(-1),
                DataAbertura = DateTime.UtcNow,
                Descricao = "Colisão de trânsito",
                ValorEstimado = 15000.00m,
                Status = "EmAnalise",
                Historico = new List<HistoricoSinistroResponse>
                {
                    new HistoricoSinistroResponse { Id = Guid.NewGuid(), StatusNovo = "Aberto", Usuario = "Analista.Teste" }
                }
            };

            _sinistroQueries.ObterPorIdAsync(id, Arg.Any<CancellationToken>()).Returns(response);

            var query = new ObterSinistroQuery(id);
            var handler = new ObterSinistroQueryHandler(_sinistroQueries);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
            Assert.Single(result.Historico);
            Assert.Equal("Colisão de trânsito", result.Descricao);
        }

        [Fact]
        public async Task ObterSinistroPorId_DeveLancarNotFoundException_QuandoNaoExiste()
        {
            // Arrange
            var id = Guid.NewGuid();
            _sinistroQueries.ObterPorIdAsync(id, Arg.Any<CancellationToken>()).Returns((SinistroResponse)null!);

            var query = new ObterSinistroQuery(id);
            var handler = new ObterSinistroQueryHandler(_sinistroQueries);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(query, CancellationToken.None));
        }

        [Fact]
        public async Task ListarSinistros_DeveChamarQueriesListarComParametrosCorretos()
        {
            // Arrange
            var page = 1;
            var pageSize = 5;
            var status = "EmAnalise";
            var dataInicio = DateTime.UtcNow.AddMonths(-1);
            var dataFim = DateTime.UtcNow;
            var campoData = "ocorrencia";

            var items = new List<SinistroResponse>();
            var pagedResult = new PagedResult<SinistroResponse>(items, 0, page, pageSize);

            _sinistroQueries.ListarAsync(status, dataInicio, dataFim, campoData, page, pageSize, Arg.Any<CancellationToken>())
                .Returns(pagedResult);

            var query = new ListarSinistrosQuery(status, dataInicio, dataFim, campoData, page, pageSize);
            var handler = new ListarSinistrosQueryHandler(_sinistroQueries);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            await _sinistroQueries.Received(1).ListarAsync(
                status, dataInicio, dataFim, campoData, page, pageSize, Arg.Any<CancellationToken>());
        }
    }
}
