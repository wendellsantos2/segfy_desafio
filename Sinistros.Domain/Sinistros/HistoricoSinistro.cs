using System;
using Sinistros.Domain.Enums;
using Sinistros.Domain.SeedWork;

namespace Sinistros.Domain.Sinistros
{
    public class HistoricoSinistro : Entity
    {
        public Guid SinistroId { get; private set; }
        public StatusSinistro? StatusAnterior { get; private set; }
        public StatusSinistro StatusNovo { get; private set; }
        public DateTime DataAlteracao { get; private set; }
        public string? Motivo { get; private set; }
        public string Usuario { get; private set; }

        private HistoricoSinistro()
        {
            Usuario = null!;
        }

        internal HistoricoSinistro(Guid sinistroId, StatusSinistro? statusAnterior, StatusSinistro statusNovo, string? motivo, string usuario)
        {
            SinistroId = sinistroId;
            StatusAnterior = statusAnterior;
            StatusNovo = statusNovo;
            DataAlteracao = DateTime.UtcNow;
            Motivo = motivo;
            Usuario = usuario;
        }
    }
}
