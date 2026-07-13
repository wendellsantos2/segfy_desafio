using System;
using System.Collections.Generic;
using Sinistros.Domain.Exceptions;
using Sinistros.Domain.SeedWork;

namespace Sinistros.Domain.Sinistros
{
    public class MotivoNegativa : ValueObject
    {
        public string Texto { get; }

        public MotivoNegativa(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                throw new RegraNegocioException("O motivo da negativa não pode ser vazio.");

            var textoLimpo = texto.Trim();

            if (textoLimpo.Length < 10)
                throw new RegraNegocioException("O motivo da negativa deve conter no mínimo 10 caracteres.");

            Texto = textoLimpo;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Texto;
        }

        public override string ToString()
        {
            return Texto;
        }
    }
}
