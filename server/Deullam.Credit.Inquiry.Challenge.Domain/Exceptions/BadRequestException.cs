namespace Deullam.Credit.Inquiry.Challenge.Domain.Exceptions
{
    public class BadRequestException : BusinessException
    {
        public BadRequestException(string message) : base(ErrorCodes.BadRequest, message) { }
    }
}
