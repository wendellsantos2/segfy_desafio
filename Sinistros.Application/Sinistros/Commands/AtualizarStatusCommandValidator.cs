using System;
using FluentValidation;
using Sinistros.Domain.Enums;

namespace Sinistros.Application.Sinistros.Commands
{
    public class AtualizarStatusCommandValidator : AbstractValidator<AtualizarStatusCommand>
    {
        public AtualizarStatusCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("O Id do sinistro é obrigatório.");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("O status é obrigatório.")
                .Must(BeAValidStatus).WithMessage("O status fornecido é inválido.");

            RuleFor(x => x.Usuario)
                .NotEmpty().WithMessage("O usuário que realiza a alteração é obrigatório.")
                .MaximumLength(100).WithMessage("O nome do usuário não pode ter mais de 100 caracteres.");

            RuleFor(x => x.MotivoNegativa)
                .NotEmpty().WithMessage("O motivo da negativa é obrigatório quando o status é Negado.")
                .MinimumLength(10).WithMessage("O motivo da negativa deve ter no mínimo 10 caracteres.")
                .When(x => x.Status != null && x.Status.Equals(StatusSinistro.Negado.ToString(), StringComparison.OrdinalIgnoreCase));

            RuleFor(x => x.ValorAprovado)
                .NotNull().WithMessage("O valor aprovado é obrigatório quando o status é Encerrado.")
                .GreaterThan(0).WithMessage("O valor aprovado deve ser maior que zero.")
                .When(x => x.Status != null && x.Status.Equals(StatusSinistro.Encerrado.ToString(), StringComparison.OrdinalIgnoreCase));
        }

        private bool BeAValidStatus(string status)
        {
            return Enum.TryParse<StatusSinistro>(status, true, out _);
        }
    }
}
