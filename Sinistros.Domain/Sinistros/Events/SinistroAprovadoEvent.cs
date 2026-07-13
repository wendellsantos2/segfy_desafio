using System;
using Sinistros.Domain.SeedWork;

namespace Sinistros.Domain.Sinistros.Events
{
    public class SinistroAprovadoEvent : IDomainEvent
    {
        public Guid SinistroId { get; }
        public Guid ApoliceId { get; }
        public DateTime OcorreuEm { get; }

        public SinistroAprovadoEvent(Guid sinistroId, Guid apoliceId)
        {
            SinistroId = sinistroId;
            ApoliceId = apoliceId;
            OcorreuEm = DateTime.UtcNow;
        }
    }
}
