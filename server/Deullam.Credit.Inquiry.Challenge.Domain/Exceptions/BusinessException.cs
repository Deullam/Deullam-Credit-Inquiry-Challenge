namespace Deullam.Credit.Inquiry.Challenge.Domain.Exceptions
{
    /// <summary>
    /// Represents the base class for all business exceptions in the application.Classe base para todas as exceções de negócio da aplicação.
    /// </summary>
    /// <remarks>This exception is intended to encapsulate errors related to business rules or domain
    /// logic. Derived classes should represent specific business exceptions, providing additional context or
    /// details as needed. The <see cref="ErrorCode"/> property can be used to identify the specific error
    /// condition.</remarks>
    public abstract class BusinessException : Exception
    {
        protected BusinessException(ErrorCodes errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }

        public ErrorCodes ErrorCode { get; }
    }
}
