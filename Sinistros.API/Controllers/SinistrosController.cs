using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sinistros.Application.DTOs;
using Sinistros.Application.Sinistros.Queries;
using Sinistros.Application.Sinistros.Commands;

namespace Sinistros.API.Controllers
{
    /// <summary>
    /// Controller para gerenciamento de sinistros.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class SinistrosController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SinistrosController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Abre um novo sinistro associado a uma apólice ativa.
        /// </summary>
        /// <param name="command">Dados necessários para a abertura do sinistro.</param>
        /// <returns>O sinistro criado com seus dados iniciais de histórico.</returns>
        /// <response code="201">Sinistro aberto com sucesso.</response>
        /// <response code="400">Dados do payload inválidos (ex: data futura, valor negativo).</response>
        /// <response code="404">A apólice fornecida não foi encontrada.</response>
        /// <response code="422">Regra de negócio violada (ex: apólice suspensa ou cancelada).</response>
        [HttpPost]
        [ProducesResponseType(typeof(SinistroResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Abrir([FromBody] AbrirSinistroCommand command)
        {
            var response = await _mediator.Send(command);
            return CreatedAtAction(nameof(ObterPorId), new { id = response.Id }, response);
        }

        /// <summary>
        /// Obtém os detalhes de um sinistro por ID, incluindo seu histórico completo.
        /// </summary>
        /// <param name="id">ID único do sinistro.</param>
        /// <returns>Dados detalhados do sinistro.</returns>
        /// <response code="200">Retorna o sinistro solicitado.</response>
        /// <response code="404">Sinistro não encontrado.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(SinistroResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterPorId(Guid id)
        {
            var query = new ObterSinistroQuery(id);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Lista os sinistros de forma paginada e filtrada por status e período de data.
        /// </summary>
        /// <param name="status">Status do sinistro para filtragem (opcional).</param>
        /// <param name="dataInicio">Data inicial do período (opcional).</param>
        /// <param name="dataFim">Data final do período (opcional).</param>
        /// <param name="campoData">Define se o período filtra pela data de 'abertura' ou 'ocorrencia'. O padrão é 'abertura' (opcional).</param>
        /// <param name="page">Número da página para paginação (padrão é 1).</param>
        /// <param name="pageSize">Tamanho da página para paginação (padrão é 10).</param>
        /// <returns>Envelope PagedResult com a lista de sinistros e metadados de paginação.</returns>
        /// <response code="200">Lista de sinistros retornada com sucesso.</response>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<SinistroResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Listar(
            [FromQuery] string? status,
            [FromQuery] DateTime? dataInicio,
            [FromQuery] DateTime? dataFim,
            [FromQuery] string? campoData,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = new ListarSinistrosQuery(status, dataInicio, dataFim, campoData, page, pageSize);
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
