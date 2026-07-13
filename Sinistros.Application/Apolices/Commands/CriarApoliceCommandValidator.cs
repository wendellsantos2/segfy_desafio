using System;
using FluentValidation;
using Sinistros.Domain.Enums;

namespace Sinistros.Application.Apolices.Commands
{
    public class CriarApoliceCommandValidator : AbstractValidator<CriarApoliceCommand>
    {
        public CriarApoliceCommandValidator()
        {
            RuleFor(x => x.Numero)
                .NotEmpty().WithMessage("O número da apólice é obrigatório.")
                .Matches(@"^[a-zA-Z0-9]{4}-[a-zA-Z0-9]{6}$").WithMessage("O número da apólice deve seguir o formato AAAA-NNNNNN.");

            RuleFor(x => x.ClienteId)
                .NotEmpty().WithMessage("O ClienteId é obrigatório.");

            RuleFor(x => x.Ramo)
                .NotEmpty().WithMessage("O Ramo é obrigatório.")
                .Must(BeAValidRamo).WithMessage("O Ramo fornecido é inválido.");

            RuleFor(x => x.Inicio)
                .NotEmpty().WithMessage("A data de início é obrigatória.");

            RuleFor(x => x.Fim)
                .NotEmpty().WithMessage("A data de fim é obrigatória.")
                .GreaterThan(x => x.Inicio).WithMessage("A data de fim da vigência deve ser maior que a data de início.");
        }

        private bool BeAValidRamo(string ramo)
        {
            return Enum.TryParse<Ramo>(ramo, true, out _);
        }
    }
}
