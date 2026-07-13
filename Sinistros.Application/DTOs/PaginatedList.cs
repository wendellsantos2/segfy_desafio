using System.Collections.Generic;

namespace Sinistros.Application.DTOs
{
    public class PaginatedList<T>
    {
        public IEnumerable<T> Itens { get; }
        public int TotalCount { get; }
        public int Page { get; }
        public int PageSize { get; }

        public PaginatedList(IEnumerable<T> itens, int totalCount, int page, int pageSize)
        {
            Itens = itens;
            TotalCount = totalCount;
            Page = page;
            PageSize = pageSize;
        }
    }
}
