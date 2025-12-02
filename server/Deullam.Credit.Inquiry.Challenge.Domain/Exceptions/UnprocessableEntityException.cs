namespace Deullam.Credit.Inquiry.Challenge.Domain.Exceptions

{
    public class UnprocessableEntityException : BusinessException
    {
        public UnprocessableEntityException(string message) : base(ErrorCodes.UnprocessableEntity, message) { }
    }
}
