using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sinistros.Application.Apolices.Commands;
using Sinistros.Application.Apolices.Queries;
using Sinistros.Application.DTOs;

namespace Sinistros.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApolicesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ApolicesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CriarApoliceCommand command)
        {
            var id = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<ApoliceResponse>>> List(
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = new ListarApolicesQuery(status, page, pageSize);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApoliceResponse>> GetById(Guid id)
        {
            var query = new ObterApolicePorIdQuery(id);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateApoliceStatusRequest request)
        {
            var command = new AtualizarStatusApoliceCommand(id, request.Status);
            await _mediator.Send(command);
            return NoContent();
        }
    }

    public record UpdateApoliceStatusRequest(string Status);
}
