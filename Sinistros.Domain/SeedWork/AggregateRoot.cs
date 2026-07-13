using System.Collections.Generic;

namespace Sinistros.Domain.SeedWork
{
    public abstract class AggregateRoot : Entity
    {
        private readonly List<IDomainEvent> _domainEvents = new();

        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        public void AdicionarEvento(IDomainEvent evento)
        {
            _domainEvents.Add(evento);
        }

        public void LimparEventos()
        {
            _domainEvents.Clear();
        }
    }
}
