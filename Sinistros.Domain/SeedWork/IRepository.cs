using System.Threading;
using System.Threading.Tasks;

namespace Sinistros.Domain.SeedWork
{
    public interface IRepository<T> where T : AggregateRoot
    {
    }

    public interface IUnitOfWork
    {
        Task<bool> CommitAsync(CancellationToken cancellationToken = default);
    }
}
