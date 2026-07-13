using MediatR;
using Sinistros.Domain.SeedWork;

namespace Sinistros.Infrastructure.Persistence.Events
{
    public class DomainEventNotificationAdapter : INotification
    {
        public IDomainEvent DomainEvent { get; }

        public DomainEventNotificationAdapter(IDomainEvent domainEvent)
        {
            DomainEvent = domainEvent;
        }
    }
}
