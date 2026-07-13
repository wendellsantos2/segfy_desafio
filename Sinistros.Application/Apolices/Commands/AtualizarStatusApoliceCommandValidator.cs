using FluentValidation;

namespace Sinistros.Application.Apolices.Commands
{
    public class AtualizarStatusApoliceCommandValidator : AbstractValidator<AtualizarStatusApoliceCommand>
    {
        public AtualizarStatusApoliceCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("O Id da apólice é obrigatório.");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("O status é obrigatório.")
                .Must(s => s.Equals("Ativa", System.StringComparison.OrdinalIgnoreCase) || 
                           s.Equals("Suspensa", System.StringComparison.OrdinalIgnoreCase) || 
                           s.Equals("Cancelada", System.StringComparison.OrdinalIgnoreCase))
                .WithMessage("O status fornecido é inválido. Valores aceitos: Ativa, Suspensa, Cancelada.");
        }
    }
}
