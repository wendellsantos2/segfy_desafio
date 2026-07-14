import { useState } from 'react';
import { Link } from 'react-router-dom';
import { Plus, Filter } from 'lucide-react';
import { useSinistros } from '../../hooks';
import { StatusBadge } from '../ui/StatusBadge';
import { Pagination } from '../ui/Pagination';
import { ErrorAlert, EmptyState, LoadingSpinner } from '../ui/Feedback';
import { AbrirSinistroModal } from './AbrirSinistroModal';
import type { StatusSinistro } from '../../types';

export function SinistrosList() {
  const [page, setPage] = useState(1);
  const [statusFiltro, setStatusFiltro] = useState<StatusSinistro | ''>('');
  const [isModalOpen, setIsModalOpen] = useState(false);

  const { data, isLoading, error } = useSinistros({
    page,
    pageSize: 10,
    status: statusFiltro || undefined,
  });

  return (
    <div className="p-8 max-w-7xl mx-auto animate-fade-in space-y-6">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-slate-100">Sinistros</h1>
          <p className="text-sm text-slate-400">Gerencie e acompanhe os sinistros abertos.</p>
        </div>
        <button className="btn-primary" onClick={() => setIsModalOpen(true)}>
          <Plus className="w-4 h-4" />
          Novo Sinistro
        </button>
      </div>

      <div className="card">
        {/* Toolbar de filtros */}
        <div className="p-4 border-b border-surface-800 flex flex-wrap gap-4 items-center">
          <div className="flex items-center gap-2">
            <Filter className="w-4 h-4 text-slate-500" />
            <select
              className="select w-40"
              value={statusFiltro}
              onChange={(e) => {
                setStatusFiltro(e.target.value as StatusSinistro | '');
                setPage(1);
              }}
            >
              <option value="">Todos os status</option>
              <option value="Aberto">Aberto</option>
              <option value="EmAnalise">Em Análise</option>
              <option value="Aprovado">Aprovado</option>
              <option value="Negado">Negado</option>
              <option value="Encerrado">Encerrado</option>
            </select>
          </div>
        </div>

        {/* Conteúdo */}
        <div className="min-h-[400px]">
          {isLoading && <LoadingSpinner />}
          
          {error && !isLoading && (
            <div className="p-6">
              <ErrorAlert message="Não foi possível carregar os sinistros. Verifique a conexão com a API." />
            </div>
          )}

          {!isLoading && !error && data?.items.length === 0 && (
            <EmptyState message="Nenhum sinistro encontrado para os filtros selecionados." />
          )}

          {!isLoading && !error && data && data.items.length > 0 && (
            <div className="overflow-x-auto">
              <table className="w-full text-left border-collapse">
                <thead>
                  <tr className="border-b border-surface-800 text-xs font-semibold text-slate-400 uppercase tracking-wider bg-surface-900/50">
                    <th className="px-6 py-4">ID</th>
                    <th className="px-6 py-4">Apólice</th>
                    <th className="px-6 py-4">Data Abertura</th>
                    <th className="px-6 py-4">Valor Estimado</th>
                    <th className="px-6 py-4">Status</th>
                    <th className="px-6 py-4 text-right">Ações</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-surface-800 text-sm">
                  {data.items.map((sinistro) => (
                    <tr key={sinistro.id} className="table-row-hover">
                      <td className="px-6 py-4 font-medium text-slate-300">
                        {sinistro.id.substring(0, 8)}...
                      </td>
                      <td className="px-6 py-4 text-slate-400">
                        {sinistro.apoliceId.substring(0, 8)}...
                      </td>
                      <td className="px-6 py-4 text-slate-400">
                        {new Date(sinistro.dataAbertura).toLocaleDateString('pt-BR')}
                      </td>
                      <td className="px-6 py-4 font-medium text-slate-300">
                        R$ {sinistro.valorEstimado.toLocaleString('pt-BR', { minimumFractionDigits: 2 })}
                      </td>
                      <td className="px-6 py-4">
                        <StatusBadge status={sinistro.status} />
                      </td>
                      <td className="px-6 py-4 text-right">
                        <Link to={`/sinistros/${sinistro.id}`} className="btn-ghost btn-sm">
                          Detalhes
                        </Link>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>

        {/* Footer com Paginação */}
        {!isLoading && !error && data && (
          <Pagination paged={data} onPageChange={setPage} />
        )}
      </div>

      <AbrirSinistroModal isOpen={isModalOpen} onClose={() => setIsModalOpen(false)} />
    </div>
  );
}
