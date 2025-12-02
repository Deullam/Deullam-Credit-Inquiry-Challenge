using Deullam.Credit.Inquiry.Challenge.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Deullam.Credit.Inquiry.Challenge.API.Middlewares
{
    /// <summary>
    /// Middleware para capturar e tratar exceções de forma global na aplicação.
    /// </summary>
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="GlobalExceptionHandlerMiddleware"/>.
        /// </summary>
        /// <param name="next">O próximo delegate no pipeline da requisição.</param>
        /// <param name="logger">O logger para registrar informações de erro.</param>
        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Invoca o middleware, envolvendo a chamada ao próximo delegate em um bloco try-catch.
        /// </summary>
        /// <param name="context">O contexto HTTP da requisição atual.</param>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro inesperado: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// Trata a exceção capturada, determinando o status code e a mensagem de erro apropriados para a resposta.
        /// </summary>
        /// <param name="context">O contexto HTTP da requisição.</param>
        /// <param name="exception">A exceção que foi capturada.</param>
        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            int statusCode;
            string message;

            switch (exception)
            {
                case NpgsqlException npgsqlEx when npgsqlEx.IsTransient:
                    statusCode = StatusCodes.Status503ServiceUnavailable;
                    message = "O serviço de banco de dados está temporariamente indisponível. Por favor, tente novamente mais tarde.";
                    break;

                case BusinessException businessEx:
                    statusCode = (int)businessEx.ErrorCode;
                    message = businessEx.Message;
                    break;

                default:
                    statusCode = StatusCodes.Status500InternalServerError;
                    message = "Ocorreu um erro interno inesperado no servidor.";
                    break;
            }

            context.Response.StatusCode = statusCode;

            var errorResponse = new
            {
                StatusCode = statusCode,
                Message = message
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
        }
    }
}
