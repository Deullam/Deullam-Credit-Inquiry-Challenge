namespace Deullam.Credit.Inquiry.Challenge.Domain.Exceptions
{
    public class ServiceUnavailableException : BusinessException
    {
        public ServiceUnavailableException(string message)
            : base(ErrorCodes.ServiceUnavailable, message) { }
    }
}
