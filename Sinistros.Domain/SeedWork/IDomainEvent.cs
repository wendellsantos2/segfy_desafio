using System;

namespace Sinistros.Domain.SeedWork
{
    public interface IDomainEvent
    {
        DateTime OcorreuEm { get; }
    }
}
