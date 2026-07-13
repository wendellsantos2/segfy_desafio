using System;
using System.Collections.Generic;
using Sinistros.Domain.Exceptions;
using Sinistros.Domain.SeedWork;

namespace Sinistros.Domain.SeedWork
{
    public class Dinheiro : ValueObject
    {
        public decimal Valor { get; }
        public string Moeda { get; }

        public Dinheiro(decimal valor, string moeda = "BRL")
        {
            if (valor < 0)
                throw new RegraNegocioException("O valor monetário não pode ser negativo.");

            if (string.IsNullOrWhiteSpace(moeda))
                throw new RegraNegocioException("A moeda deve ser informada.");

            Valor = valor;
            Moeda = moeda.ToUpperInvariant();
        }

        public static Dinheiro Zero() => new(0);

        public static Dinheiro operator +(Dinheiro a, Dinheiro b)
        {
            if (a.Moeda != b.Moeda)
                throw new RegraNegocioException($"Não é possível somar moedas diferentes: {a.Moeda} e {b.Moeda}.");

            return new Dinheiro(a.Valor + b.Valor, a.Moeda);
        }

        public static bool operator >(Dinheiro a, Dinheiro b)
        {
            if (a.Moeda != b.Moeda)
                throw new RegraNegocioException($"Não é possível comparar moedas diferentes: {a.Moeda} e {b.Moeda}.");

            return a.Valor > b.Valor;
        }

        public static bool operator <(Dinheiro a, Dinheiro b)
        {
            if (a.Moeda != b.Moeda)
                throw new RegraNegocioException($"Não é possível comparar moedas diferentes: {a.Moeda} e {b.Moeda}.");

            return a.Valor < b.Valor;
        }

        public static bool operator >=(Dinheiro a, Dinheiro b)
        {
            if (a.Moeda != b.Moeda)
                throw new RegraNegocioException($"Não é possível comparar moedas diferentes: {a.Moeda} e {b.Moeda}.");

            return a.Valor >= b.Valor;
        }

        public static bool operator <=(Dinheiro a, Dinheiro b)
        {
            if (a.Moeda != b.Moeda)
                throw new RegraNegocioException($"Não é possível comparar moedas diferentes: {a.Moeda} e {b.Moeda}.");

            return a.Valor <= b.Valor;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Valor;
            yield return Moeda;
        }

        public override string ToString()
        {
            return $"{Moeda} {Valor:F2}";
        }
    }
}
