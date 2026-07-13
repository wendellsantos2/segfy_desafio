using System;
using System.Collections.Generic;
using Sinistros.Domain.Exceptions;
using Sinistros.Domain.SeedWork;

namespace Sinistros.Domain.Apolices
{
    public class PeriodoVigencia : ValueObject
    {
        public DateTime Inicio { get; }
        public DateTime Fim { get; }

        public PeriodoVigencia(DateTime inicio, DateTime fim)
        {
            if (fim <= inicio)
                throw new RegraNegocioException("A data de término da vigência deve ser maior que a data de início.");

            Inicio = inicio;
            Fim = fim;
        }

        public bool EstaVigenteEm(DateTime data)
        {
            return data >= Inicio && data <= Fim;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Inicio;
            yield return Fim;
        }
    }
}
