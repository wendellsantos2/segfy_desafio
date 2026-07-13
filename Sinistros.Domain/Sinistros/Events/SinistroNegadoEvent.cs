using System;
using Sinistros.Domain.SeedWork;

namespace Sinistros.Domain.Sinistros.Events
{
    public class SinistroNegadoEvent : IDomainEvent
    {
        public Guid SinistroId { get; }
        public Guid ApoliceId { get; }
        public MotivoNegativa Motivo { get; }
        public DateTime OcorreuEm { get; }

        public SinistroNegadoEvent(Guid sinistroId, Guid apoliceId, MotivoNegativa motivo)
        {
            SinistroId = sinistroId;
            ApoliceId = apoliceId;
            Motivo = motivo;
            OcorreuEm = DateTime.UtcNow;
        }
    }
}
