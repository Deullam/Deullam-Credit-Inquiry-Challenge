namespace Deullam.Credit.Inquiry.Challenge.Domain.Exceptions
{
    public class ConflictException : BusinessException
    {
        public ConflictException(string message) : base(ErrorCodes.Conflict, message) { }
    }
}
