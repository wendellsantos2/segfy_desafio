using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Sinistros.Domain.Exceptions;
using Sinistros.Domain.SeedWork;

namespace Sinistros.Domain.Apolices
{
    public class NumeroApolice : ValueObject
    {
        private static readonly Regex FormatoRegex = new(@"^[a-zA-Z0-9]{4}-[a-zA-Z0-9]{6}$", RegexOptions.Compiled);

        public string Valor { get; }

        public NumeroApolice(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new RegraNegocioException("O número da apólice não pode ser vazio.");

            if (!FormatoRegex.IsMatch(valor))
                throw new RegraNegocioException("O número da apólice deve estar no formato AAAA-NNNNNN (ex: 2026-123456).");

            Valor = valor.ToUpperInvariant();
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Valor;
        }
    }
}
