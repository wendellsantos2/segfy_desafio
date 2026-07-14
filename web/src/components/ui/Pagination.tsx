import { ChevronLeft, ChevronRight } from 'lucide-react';
import type { PagedResult } from '../../types';

interface PaginationProps {
  paged: Pick<PagedResult<unknown>, 'page' | 'totalPages' | 'totalItems' | 'hasNext' | 'hasPrevious'>;
  onPageChange: (page: number) => void;
}

export function Pagination({ paged, onPageChange }: PaginationProps) {
  if (paged.totalPages <= 1) return null;

  return (
    <div className="flex items-center justify-between px-4 py-3 border-t border-surface-800 text-sm text-slate-400">
      <span>{paged.totalItems} registro{paged.totalItems !== 1 ? 's' : ''}</span>
      <div className="flex items-center gap-2">
        <button
          className="btn btn-ghost btn-sm"
          disabled={!paged.hasPrevious}
          onClick={() => onPageChange(paged.page - 1)}
        >
          <ChevronLeft className="w-4 h-4" />
          Anterior
        </button>
        <span className="px-2 py-1 rounded bg-surface-800 text-slate-300 text-xs font-medium">
          {paged.page} / {paged.totalPages}
        </span>
        <button
          className="btn btn-ghost btn-sm"
          disabled={!paged.hasNext}
          onClick={() => onPageChange(paged.page + 1)}
        >
          Próxima
          <ChevronRight className="w-4 h-4" />
        </button>
      </div>
    </div>
  );
}
