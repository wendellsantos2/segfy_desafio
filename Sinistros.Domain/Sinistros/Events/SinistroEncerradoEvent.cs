using System;
using Sinistros.Domain.SeedWork;

namespace Sinistros.Domain.Sinistros.Events
{
    public class SinistroEncerradoEvent : IDomainEvent
    {
        public Guid SinistroId { get; }
        public Guid ApoliceId { get; }
        public Dinheiro ValorAprovado { get; }
        public DateTime OcorreuEm { get; }

        public SinistroEncerradoEvent(Guid sinistroId, Guid apoliceId, Dinheiro valorAprovado)
        {
            SinistroId = sinistroId;
            ApoliceId = apoliceId;
            ValorAprovado = valorAprovado;
            OcorreuEm = DateTime.UtcNow;
        }
    }
}
