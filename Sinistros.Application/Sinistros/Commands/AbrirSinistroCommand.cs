using System;
using MediatR;
using Sinistros.Application.DTOs;

namespace Sinistros.Application.Sinistros.Commands
{
    public record AbrirSinistroCommand(
        Guid ApoliceId,
        DateTime DataOcorrencia,
        string Descricao,
        decimal ValorEstimado
    ) : IRequest<SinistroResponse>;
}
