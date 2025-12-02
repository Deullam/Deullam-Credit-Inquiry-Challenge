namespace Deullam.Credit.Inquiry.Challenge.Domain.Exceptions
{
    /// <summary>
    /// Represents an exception that is thrown when a requested resource cannot be found.
    /// Exceção específica para ser lançada quando um recurso não é encontrado.
    /// </summary>
    /// <remarks>This exception is typically used to indicate that an operation failed because the specified
    /// resource does not exist. It includes a default error code of <see cref="ErrorCodes.NotFound"/>.</remarks>
    public class NotFoundException : BusinessException
    {
        // Construtor padrão com uma mensagem genérica.
        public NotFoundException() : base(ErrorCodes.NotFound, "O recurso solicitado não foi encontrado.") { }

        // Construtor que permite uma mensagem personalizada.
        public NotFoundException(string message) : base(ErrorCodes.NotFound, message) { }
    }
}
