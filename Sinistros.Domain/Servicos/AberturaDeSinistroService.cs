using System;
using Sinistros.Domain.Apolices;
using Sinistros.Domain.Exceptions;
using Sinistros.Domain.SeedWork;
using Sinistros.Domain.Sinistros;

namespace Sinistros.Domain.Servicos
{
    public class AberturaDeSinistroService
    {
        public Sinistro Abrir(Apolice apolice, DateTime dataOcorrencia, string descricao, Dinheiro valorEstimado, string usuario = "Sistema")
        {
            if (apolice == null)
                throw new RegraNegocioException("A apólice é obrigatória para a abertura do sinistro.");

            if (!apolice.EstaAtiva())
                throw new RegraNegocioException("Sinistro so pode ser aberto em apolice ativa");

            return Sinistro.Abrir(apolice.Id, dataOcorrencia, descricao, valorEstimado, usuario);
        }
    }
}
