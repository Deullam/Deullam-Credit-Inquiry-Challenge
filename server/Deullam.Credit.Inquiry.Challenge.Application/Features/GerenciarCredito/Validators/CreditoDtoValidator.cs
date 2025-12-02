using FluentValidation;

namespace Deullam.Credit.Inquiry.Challenge.Application.Features.GerenciarCredito.Validators
{
    public class CreditoDtoValidator : AbstractValidator<CreditoDto>
    {
        public CreditoDtoValidator()
        {
            RuleFor(c => c.NumeroCredito)
                .NotEmpty().WithMessage("O número do crédito é obrigatório.")
                .MaximumLength(50).WithMessage("O número do crédito não pode exceder 50 caracteres.");

            RuleFor(c => c.NumeroNfse)
                .NotEmpty().WithMessage("O número da NFS-e é obrigatório.")
                .MaximumLength(50).WithMessage("O número da NFS-e não pode exceder 50 caracteres.");

            RuleFor(c => c.DataConstituicao)
                .NotEmpty().WithMessage("A data de constituição é obrigatória.")
                .LessThanOrEqualTo(System.DateTime.Now).WithMessage("A data de constituição não pode ser no futuro.");

            RuleFor(c => c.ValorIssqn)
                .GreaterThan(0).WithMessage("O valor do ISSQN deve ser maior que zero.");

            RuleFor(c => c.TipoCredito)
                .NotEmpty().WithMessage("O tipo de crédito é obrigatório.");

            RuleFor(c => c.SimplesNacional)
                .NotEmpty().WithMessage("A informação sobre Simples Nacional é obrigatória.")
                .Must(s => s.Equals("Sim", System.StringComparison.OrdinalIgnoreCase) || s.Equals("Não", System.StringComparison.OrdinalIgnoreCase))
                .WithMessage("O campo Simples Nacional deve ser 'Sim' ou 'Não'.");

            RuleFor(c => c.Aliquota)
                .InclusiveBetween(0, 100).WithMessage("A alíquota deve estar entre 0 e 100.");
        }
    }
}
