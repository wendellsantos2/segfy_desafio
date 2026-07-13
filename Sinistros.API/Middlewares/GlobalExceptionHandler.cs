using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sinistros.Application.Exceptions;
using Sinistros.Domain.Exceptions;

namespace Sinistros.API.Middlewares
{
    /// <summary>
    /// Implementa IExceptionHandler (.NET 8) para traduzir exceções de domínio/aplicação
    /// em respostas ProblemDetails (RFC 7807) na borda da aplicação.
    ///
    /// POR QUE A TRADUÇÃO MORA NA BORDA?
    /// O domínio e a aplicação não conhecem HTTP. Eles lançam exceções semânticas
    /// (RegraNegocioException, NotFoundException) que descrevem O QUE deu errado no
    /// contexto do negócio. A tarefa de mapear isso para um status HTTP (422, 404, 400)
    /// é responsabilidade exclusiva da camada de apresentação (API). Isso garante que:
    /// 1. O domínio permanece independente de qualquer protocolo ou framework.
    /// 2. A mesma lógica de negócio pode ser exposta por gRPC, mensageria, CLI, etc.
    ///    sem alterar uma linha de código de domínio.
    /// 3. O stack trace nunca vaza para o cliente — o handler captura tudo e devolve
    ///    apenas as informações seguras via ProblemDetails.
    /// </summary>
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "Exceção não tratada: {Message}", exception.Message);

            var (statusCode, title, detail, errors) = exception switch
            {
                ValidationException ve => (
                    StatusCodes.Status400BadRequest,
                    "Erro de validação",
                    ve.Message,
                    (object?)ve.Errors),

                NotFoundException nfe => (
                    StatusCodes.Status404NotFound,
                    "Recurso não encontrado",
                    nfe.Message,
                    (object?)null),

                RegraNegocioException rne => (
                    StatusCodes.Status422UnprocessableEntity,
                    "Regra de negócio violada",
                    rne.Message,
                    (object?)null),

                _ => (
                    StatusCodes.Status500InternalServerError,
                    "Erro interno do servidor",
                    "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.",
                    (object?)null)
            };

            httpContext.Response.StatusCode = statusCode;

            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Type = $"https://httpstatuses.com/{statusCode}"
            };

            if (errors is not null)
            {
                problem.Extensions["errors"] = errors;
            }

            await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
            return true;
        }
    }
}
