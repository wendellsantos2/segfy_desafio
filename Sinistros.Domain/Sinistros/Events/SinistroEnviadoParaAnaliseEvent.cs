using System;
using Sinistros.Domain.SeedWork;

namespace Sinistros.Domain.Sinistros.Events
{
    public class SinistroEnviadoParaAnaliseEvent : IDomainEvent
    {
        public Guid SinistroId { get; }
        public Guid ApoliceId { get; }
        public DateTime OcorreuEm { get; }

        public SinistroEnviadoParaAnaliseEvent(Guid sinistroId, Guid apoliceId)
        {
            SinistroId = sinistroId;
            ApoliceId = apoliceId;
            OcorreuEm = DateTime.UtcNow;
        }
    }
}
