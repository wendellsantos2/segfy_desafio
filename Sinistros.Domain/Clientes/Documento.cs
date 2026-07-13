using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Sinistros.Domain.Exceptions;
using Sinistros.Domain.SeedWork;

namespace Sinistros.Domain.Clientes
{
    public class Documento : ValueObject
    {
        public string Numero { get; }

        public Documento(string numero)
        {
            if (string.IsNullOrWhiteSpace(numero))
                throw new RegraNegocioException("O número do documento não pode ser vazio.");

            // Remover qualquer formatação
            var apenasNumeros = Regex.Replace(numero, @"[^\d]", "");

            if (apenasNumeros.Length == 11)
            {
                if (!ValidarCpf(apenasNumeros))
                    throw new RegraNegocioException("CPF inválido.");
            }
            else if (apenasNumeros.Length == 14)
            {
                if (!ValidarCnpj(apenasNumeros))
                    throw new RegraNegocioException("CNPJ inválido.");
            }
            else
            {
                throw new RegraNegocioException("O documento deve ser um CPF (11 dígitos) ou CNPJ (14 dígitos).");
            }

            Numero = apenasNumeros;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Numero;
        }

        private static bool ValidarCpf(string cpf)
        {
            if (new string(cpf[0], 11) == cpf)
                return false;

            var multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            var multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            var tempCpf = cpf.Substring(0, 9);
            var soma = 0;

            for (var i = 0; i < 9; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];

            var resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            var digito = resto.ToString();
            tempCpf += digito;
            soma = 0;

            for (var i = 0; i < 10; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];

            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            digito += resto.ToString();

            return cpf.EndsWith(digito);
        }

        private static bool ValidarCnpj(string cnpj)
        {
            if (new string(cnpj[0], 14) == cnpj)
                return false;

            var multiplicador1 = new int[12] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            var multiplicador2 = new int[13] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            var tempCnpj = cnpj.Substring(0, 12);
            var soma = 0;

            for (var i = 0; i < 12; i++)
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador1[i];

            var resto = (soma % 11);
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            var digito = resto.ToString();
            tempCnpj += digito;
            soma = 0;

            for (var i = 0; i < 13; i++)
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador2[i];

            resto = (soma % 11);
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            digito += resto.ToString();

            return cnpj.EndsWith(digito);
        }
    }
}
