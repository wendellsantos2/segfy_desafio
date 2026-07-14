import { useState } from 'react';
import { Filter } from 'lucide-react';
import { useApolices, useAtualizarStatusApolice } from '../../hooks';
import { StatusBadge } from '../ui/StatusBadge';
import { Pagination } from '../ui/Pagination';
import { ErrorAlert, EmptyState, LoadingSpinner } from '../ui/Feedback';
import type { StatusApolice } from '../../types';

export function ApolicesList() {
  const [page, setPage] = useState(1);
  const [statusFiltro, setStatusFiltro] = useState<StatusApolice | ''>('');

  const { data, isLoading, error } = useApolices({
    page,
    pageSize: 10,
    status: statusFiltro || undefined,
  });



  // Componente interno para botão de ação
  const BotaoAcao = ({ apolice }: { apolice: any }) => {
    const mut = useAtualizarStatusApolice(apolice.id);
    const executar = async (acao: 'Suspender' | 'Reativar' | 'Cancelar') => {
      if (!window.confirm(`Confirma ${acao.toLowerCase()}?`)) return;
      try {
        await mut.mutateAsync({ acao });
      } catch (e: any) {
        alert(e?.friendlyMessage || 'Erro.');
      }
    };

    if (apolice.status === 'Cancelada') return null;

    return (
      <div className="flex justify-end gap-2">
        {apolice.status === 'Ativa' && (
          <button onClick={() => executar('Suspender')} className="btn-secondary btn-sm" disabled={mut.isPending}>
            Suspender
          </button>
        )}
        {apolice.status === 'Suspensa' && (
          <button onClick={() => executar('Reativar')} className="btn-primary btn-sm" disabled={mut.isPending}>
            Reativar
          </button>
        )}
        <button onClick={() => executar('Cancelar')} className="btn-danger btn-sm" disabled={mut.isPending}>
          Cancelar
        </button>
      </div>
    );
  };

  return (
    <div className="p-8 max-w-7xl mx-auto animate-fade-in space-y-6">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-slate-100">Apólices</h1>
          <p className="text-sm text-slate-400">Gerencie a vigência e status das apólices.</p>
        </div>
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
                setStatusFiltro(e.target.value as StatusApolice | '');
                setPage(1);
              }}
            >
              <option value="">Todos os status</option>
              <option value="Ativa">Ativa</option>
              <option value="Suspensa">Suspensa</option>
              <option value="Cancelada">Cancelada</option>
            </select>
          </div>
        </div>

        {/* Conteúdo */}
        <div className="min-h-[400px]">
          {isLoading && <LoadingSpinner />}
          
          {error && !isLoading && (
            <div className="p-6">
              <ErrorAlert message="Não foi possível carregar as apólices." />
            </div>
          )}

          {!isLoading && !error && data?.items.length === 0 && (
            <EmptyState message="Nenhuma apólice encontrada." />
          )}

          {!isLoading && !error && data && data.items.length > 0 && (
            <div className="overflow-x-auto">
              <table className="w-full text-left border-collapse">
                <thead>
                  <tr className="border-b border-surface-800 text-xs font-semibold text-slate-400 uppercase tracking-wider bg-surface-900/50">
                    <th className="px-6 py-4">Número</th>
                    <th className="px-6 py-4">Ramo</th>
                    <th className="px-6 py-4">Vigência Inicial</th>
                    <th className="px-6 py-4">Vigência Final</th>
                    <th className="px-6 py-4">Status</th>
                    <th className="px-6 py-4 text-right">Ações</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-surface-800 text-sm">
                  {data.items.map((apolice) => (
                    <tr key={apolice.id} className="table-row-hover">
                      <td className="px-6 py-4 font-medium text-slate-300 font-mono">
                        {apolice.numero}
                      </td>
                      <td className="px-6 py-4 text-slate-400">
                        {apolice.ramo}
                      </td>
                      <td className="px-6 py-4 text-slate-400">
                        {new Date(apolice.vigenciaInicio).toLocaleDateString('pt-BR')}
                      </td>
                      <td className="px-6 py-4 text-slate-400">
                        {new Date(apolice.vigenciaFim).toLocaleDateString('pt-BR')}
                      </td>
                      <td className="px-6 py-4">
                        <StatusBadge status={apolice.status} />
                      </td>
                      <td className="px-6 py-4 text-right">
                        <BotaoAcao apolice={apolice} />
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
    </div>
  );
}
