using System;
using Sinistros.Domain.Enums;
using Sinistros.Domain.Exceptions;
using Sinistros.Domain.SeedWork;

namespace Sinistros.Domain.Apolices
{
    public class Apolice : AggregateRoot
    {
        public NumeroApolice Numero { get; private set; }
        public Guid ClienteId { get; private set; }
        public Ramo Ramo { get; private set; }
        public StatusApolice Status { get; private set; }
        public PeriodoVigencia Vigencia { get; private set; }

        private Apolice()
        {
            Numero = null!;
            Vigencia = null!;
        }

        private Apolice(NumeroApolice numero, Guid clienteId, Ramo ramo, PeriodoVigencia vigencia)
        {
            Numero = numero ?? throw new RegraNegocioException("O número da apólice é obrigatório.");
            
            if (clienteId == Guid.Empty)
                throw new RegraNegocioException("O ClienteId é obrigatório.");

            ClienteId = clienteId;
            Ramo = ramo;
            Vigencia = vigencia ?? throw new RegraNegocioException("O período de vigência é obrigatório.");
            Status = StatusApolice.Ativa;
        }

        public static Apolice Emitir(NumeroApolice numero, Guid clienteId, Ramo ramo, PeriodoVigencia vigencia)
        {
            return new Apolice(numero, clienteId, ramo, vigencia);
        }

        public void Suspender()
        {
            if (Status == StatusApolice.Cancelada)
                throw new RegraNegocioException("Não é possível suspender uma apólice cancelada.");

            if (Status == StatusApolice.Suspensa)
                throw new RegraNegocioException("A apólice já está suspensa.");

            if (Status != StatusApolice.Ativa)
                throw new RegraNegocioException($"Transição de status inválida de {Status} para Suspensa.");

            Status = StatusApolice.Suspensa;
        }

        public void Reativar()
        {
            if (Status == StatusApolice.Cancelada)
                throw new RegraNegocioException("Não é possível reativar uma apólice cancelada.");

            if (Status == StatusApolice.Ativa)
                throw new RegraNegocioException("A apólice já está ativa.");

            if (Status != StatusApolice.Suspensa)
                throw new RegraNegocioException($"Transição de status inválida de {Status} para Ativa.");

            Status = StatusApolice.Ativa;
        }

        public void Cancelar()
        {
            if (Status == StatusApolice.Cancelada)
                throw new RegraNegocioException("A apólice já está cancelada.");

            if (Status != StatusApolice.Ativa && Status != StatusApolice.Suspensa)
                throw new RegraNegocioException($"Transição de status inválida de {Status} para Cancelada.");

            Status = StatusApolice.Cancelada;
        }

        public bool EstaAtiva()
        {
            return Status == StatusApolice.Ativa;
        }
    }
}
