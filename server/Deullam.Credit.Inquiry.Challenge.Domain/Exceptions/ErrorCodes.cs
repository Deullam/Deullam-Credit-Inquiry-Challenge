namespace Deullam.Credit.Inquiry.Challenge.Domain.Exceptions
{
    /// <summary>
    /// Enum para padronizar os códigos de erro de negócio.
    /// </summary>
    public enum ErrorCodes
    {
        // Códigos HTTP-like para erros comuns.
        BadRequest = 400,
        Unauthorized = 401,
        Forbidden = 403,
        NotFound = 404,
        MethodNotAllowed = 405,
        Conflict = 409, // Renomeado de AlreadyExists para o termo HTTP padrão
        UnprocessableEntity = 422, // Renomeado de InvalidObject para o termo HTTP padrão

        // Códigos para erros de servidor
        InternalServerError = 500, // Renomeado de Unhandled
        ServiceUnavailable = 503,
    }
}
