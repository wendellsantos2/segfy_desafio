using System;
using Sinistros.Domain.SeedWork;

namespace Sinistros.Domain.Sinistros
{
    public class SinistroAbertoEvent : IDomainEvent
    {
        public Guid SinistroId { get; }
        public Guid ApoliceId { get; }
        public DateTime OcorreuEm { get; }

        public SinistroAbertoEvent(Guid sinistroId, Guid apoliceId)
        {
            SinistroId = sinistroId;
            ApoliceId = apoliceId;
            OcorreuEm = DateTime.UtcNow;
        }
    }
}
