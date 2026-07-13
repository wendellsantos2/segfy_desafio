using System;
using Sinistros.Domain.Enums;
using Sinistros.Domain.SeedWork;

namespace Sinistros.Domain.Sinistros
{
    public class SinistroStatusAlteradoEvent : IDomainEvent
    {
        public Guid SinistroId { get; }
        public StatusSinistro? StatusAnterior { get; }
        public StatusSinistro StatusNovo { get; }
        public DateTime OcorreuEm { get; }

        public SinistroStatusAlteradoEvent(Guid sinistroId, StatusSinistro? statusAnterior, StatusSinistro statusNovo)
        {
            SinistroId = sinistroId;
            StatusAnterior = statusAnterior;
            StatusNovo = statusNovo;
            OcorreuEm = DateTime.UtcNow;
        }
    }
}
