using System;
using Sinistros.Domain.Exceptions;
using Sinistros.Domain.SeedWork;

namespace Sinistros.Domain.Clientes
{
    public class Cliente : AggregateRoot
    {
        public string Nome { get; private set; }
        public Documento Documento { get; private set; }

        private Cliente()
        {
            Nome = null!;
            Documento = null!;
        }

        private Cliente(string nome, Documento documento)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new RegraNegocioException("O nome do cliente não pode ser vazio ou nulo.");

            Nome = nome.Trim();
            Documento = documento ?? throw new RegraNegocioException("O documento do cliente é obrigatório.");
        }

        public static Cliente Criar(string nome, Documento documento)
        {
            return new Cliente(nome, documento);
        }
    }
}
