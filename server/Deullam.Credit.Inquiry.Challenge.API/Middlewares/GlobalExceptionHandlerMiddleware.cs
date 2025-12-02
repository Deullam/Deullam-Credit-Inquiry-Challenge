using Deullam.Credit.Inquiry.Challenge.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Deullam.Credit.Inquiry.Challenge.API.Middlewares
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Tenta executar o próximo middleware no pipeline.
                // Se nenhuma exceção ocorrer, este bloco termina e a resposta normal é enviada.
                await _next(context);
            }
            catch (BusinessException ex)
            {
                // Captura especificamente as nossas exceções de negócio.
                await HandleBusinessExceptionAsync(context, ex);
            }
            catch (Exception ex)
            {
                // Captura qualquer outra exceção não esperada (erros de sistema, etc.).
                await HandleGenericExceptionAsync(context, ex);
            }
        }

        private static Task HandleBusinessExceptionAsync(HttpContext context, BusinessException exception)
        {
            // Usa o ErrorCode da exceção para definir o status HTTP da resposta.
            context.Response.StatusCode = (int)exception.ErrorCode;
            context.Response.ContentType = "application/json";

            // Cria um objeto de erro padronizado para a resposta.
            var errorResponse = new
            {
                StatusCode = context.Response.StatusCode,
                Message = exception.Message
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
        }

        private static Task HandleGenericExceptionAsync(HttpContext context, Exception exception)
        {
            // Para erros não esperados, sempre retorna um 500 Internal Server Error.
            context.Response.StatusCode = (int)ErrorCodes.InternalServerError;
            context.Response.ContentType = "application/json";

            // Em ambiente de desenvolvimento, podemos incluir mais detalhes do erro.
            // Em produção, apenas uma mensagem genérica para não expor detalhes internos.
            var message = "Ocorreu um erro inesperado no servidor.";
#if DEBUG
            message = exception.ToString(); // Em Debug, mostra a stack trace completa.
#endif

            var errorResponse = new
            {
                StatusCode = context.Response.StatusCode,
                Message = message
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
        }
    }
}
