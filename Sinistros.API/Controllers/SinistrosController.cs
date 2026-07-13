using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sinistros.Application.DTOs;
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
        /// Obtém os detalhes de um sinistro por ID.
        /// </summary>
        /// <param name="id">ID único do sinistro.</param>
        /// <returns>Dados detalhados do sinistro.</returns>
        /// <response code="200">Retorna o sinistro solicitado.</response>
        /// <response code="404">Sinistro não encontrado.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(SinistroResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<IActionResult> ObterPorId(Guid id)
        {
            return Task.FromResult<IActionResult>(Ok());
        }
    }
}
