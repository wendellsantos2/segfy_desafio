using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Sinistros.Application.Exceptions;
using Sinistros.Domain.Exceptions;

namespace Sinistros.API.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu uma exceção não tratada na execução da requisição.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var statusCode = StatusCodes.Status500InternalServerError;
            var responsePayload = new object();

            switch (exception)
            {
                case ValidationException validationEx:
                    statusCode = StatusCodes.Status400BadRequest;
                    responsePayload = new
                    {
                        message = validationEx.Message,
                        errors = validationEx.Errors
                    };
                    break;

                case NotFoundException notFoundEx:
                    statusCode = StatusCodes.Status404NotFound;
                    responsePayload = new
                    {
                        message = notFoundEx.Message
                    };
                    break;

                case RegraNegocioException businessRuleEx:
                    statusCode = StatusCodes.Status422UnprocessableEntity;
                    responsePayload = new
                    {
                        message = businessRuleEx.Message
                    };
                    break;

                default:
                    responsePayload = new
                    {
                        message = "Ocorreu um erro interno no servidor. Por favor, tente novamente mais tarde."
                    };
                    break;
            }

            context.Response.StatusCode = statusCode;
            var json = JsonSerializer.Serialize(responsePayload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return context.Response.WriteAsync(json);
        }
    }
}
