using System;
using FluentValidation;

namespace Sinistros.Application.Sinistros.Commands
{
    public class AbrirSinistroCommandValidator : AbstractValidator<AbrirSinistroCommand>
    {
        public AbrirSinistroCommandValidator()
        {
            RuleFor(x => x.ApoliceId)
                .NotEmpty().WithMessage("O ApoliceId é obrigatório.");

            RuleFor(x => x.DataOcorrencia)
                .NotEmpty().WithMessage("A data de ocorrência é obrigatória.")
                .LessThanOrEqualTo(x => DateTime.UtcNow).WithMessage("A data de ocorrência não pode ser futura.");

            RuleFor(x => x.Descricao)
                .NotEmpty().WithMessage("A descrição é obrigatória.")
                .MaximumLength(1000).WithMessage("A descrição não pode ter mais de 1000 caracteres.");

            RuleFor(x => x.ValorEstimado)
                .GreaterThan(0).WithMessage("O valor estimado deve ser maior que zero.");
        }
    }
}
