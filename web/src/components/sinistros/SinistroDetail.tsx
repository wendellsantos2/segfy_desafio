import { useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { ArrowLeft, Clock, AlertCircle } from 'lucide-react';
import { useSinistro, useHistorico } from '../../hooks';
import { TRANSICOES_VALIDAS, type StatusSinistro } from '../../types';
import { StatusBadge } from '../ui/StatusBadge';
import { LoadingSpinner, ErrorAlert } from '../ui/Feedback';
import { AtualizarStatusModal } from './AtualizarStatusModal';

export function SinistroDetail() {
  const { id } = useParams<{ id: string }>();
  const { data: sinistro, isLoading: isLoadingS, error: errorS } = useSinistro(id!);
  const { data: historico, isLoading: isLoadingH } = useHistorico(id!);

  const [modalOpen, setModalOpen] = useState(false);
  const [statusAlvo, setStatusAlvo] = useState<StatusSinistro>('Aberto');

  if (isLoadingS || isLoadingH) return <LoadingSpinner />;
  if (errorS || !sinistro) return <div className="p-8"><ErrorAlert message="Sinistro não encontrado." /></div>;

  const transicoes = TRANSICOES_VALIDAS[sinistro.status] || [];

  function openModal(novoStatus: StatusSinistro) {
    setStatusAlvo(novoStatus);
    setModalOpen(true);
  }

  return (
    <div className="p-8 max-w-5xl mx-auto animate-fade-in space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Link to="/sinistros" className="btn-ghost btn-sm px-2">
            <ArrowLeft className="w-4 h-4" />
          </Link>
          <div>
            <div className="flex items-center gap-3">
              <h1 className="text-2xl font-bold text-slate-100">Sinistro</h1>
              <StatusBadge status={sinistro.status} />
            </div>
            <p className="text-sm text-slate-400 font-mono mt-1">{sinistro.id}</p>
          </div>
        </div>
        
        {/* Ações Contextuais */}
        <div className="flex gap-2">
          {transicoes.map(t => (
            <button
              key={t}
              onClick={() => openModal(t)}
              className={t === 'Negado' ? 'btn-danger' : 'btn-primary'}
            >
              Mover para {t === 'EmAnalise' ? 'Em Análise' : t}
            </button>
          ))}
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Detalhes Principais */}
        <div className="lg:col-span-2 space-y-6">
          <div className="card p-6">
            <h2 className="text-lg font-semibold text-slate-200 mb-4 border-b border-surface-800 pb-2">
              Detalhes do Sinistro
            </h2>
            <div className="grid grid-cols-2 gap-y-6 gap-x-4">
              <div>
                <p className="label">Apólice Vinculada</p>
                <p className="text-slate-200 font-mono text-sm">{sinistro.apoliceId}</p>
              </div>
              <div>
                <p className="label">Data de Ocorrência</p>
                <p className="text-slate-200">{new Date(sinistro.dataOcorrencia).toLocaleDateString('pt-BR')}</p>
              </div>
              <div>
                <p className="label">Data de Abertura</p>
                <p className="text-slate-200">{new Date(sinistro.dataAbertura).toLocaleString('pt-BR')}</p>
              </div>
              <div>
                <p className="label">Valor Estimado</p>
                <p className="text-slate-200 font-medium">
                  R$ {sinistro.valorEstimado.toLocaleString('pt-BR', { minimumFractionDigits: 2 })}
                </p>
              </div>
              
              <div className="col-span-2">
                <p className="label">Descrição do Evento</p>
                <div className="bg-surface-950 p-4 rounded-lg border border-surface-800 text-slate-300 text-sm whitespace-pre-wrap">
                  {sinistro.descricao}
                </div>
              </div>

              {sinistro.status === 'Encerrado' && sinistro.valorAprovado != null && (
                <div className="col-span-2 bg-emerald-500/10 border border-emerald-500/20 rounded-lg p-4 flex justify-between items-center">
                  <div>
                    <p className="text-xs text-emerald-500 font-semibold uppercase tracking-wider">Valor Aprovado</p>
                    <p className="text-emerald-400 font-bold text-xl mt-1">
                      R$ {sinistro.valorAprovado.toLocaleString('pt-BR', { minimumFractionDigits: 2 })}
                    </p>
                  </div>
                  {sinistro.dataEncerramento && (
                    <div className="text-right">
                      <p className="text-xs text-emerald-500 font-semibold uppercase tracking-wider">Encerrado em</p>
                      <p className="text-emerald-400 text-sm mt-1">
                        {new Date(sinistro.dataEncerramento).toLocaleDateString('pt-BR')}
                      </p>
                    </div>
                  )}
                </div>
              )}

              {sinistro.status === 'Negado' && sinistro.motivoNegativa && (
                <div className="col-span-2 bg-rose-500/10 border border-rose-500/20 rounded-lg p-4">
                  <div className="flex items-center gap-2 text-rose-500 mb-2">
                    <AlertCircle className="w-4 h-4" />
                    <p className="text-sm font-semibold uppercase tracking-wider">Motivo da Negativa</p>
                  </div>
                  <p className="text-rose-200 text-sm">{sinistro.motivoNegativa}</p>
                </div>
              )}
            </div>
          </div>
        </div>

        {/* Timeline Vertical */}
        <div className="lg:col-span-1">
          <div className="card p-6 h-full">
            <h2 className="text-lg font-semibold text-slate-200 mb-6 flex items-center gap-2">
              <Clock className="w-5 h-5 text-slate-400" />
              Histórico
            </h2>
            
            <div className="space-y-6 relative before:absolute before:inset-0 before:ml-5 before:-translate-x-px md:before:mx-auto md:before:translate-x-0 before:h-full before:w-0.5 before:bg-gradient-to-b before:from-transparent before:via-surface-700 before:to-transparent">
              {historico?.map((h) => (
                <div key={h.id} className="relative flex items-center justify-between md:justify-normal md:odd:flex-row-reverse group is-active">
                  <div className="flex items-center justify-center w-10 h-10 rounded-full border-4 border-surface-900 bg-surface-700 text-slate-400 group-hover:text-brand-400 group-hover:bg-brand-900/30 transition-colors shadow shrink-0 md:order-1 md:group-odd:-translate-x-1/2 md:group-even:translate-x-1/2 z-10">
                    <div className="w-2 h-2 rounded-full bg-current" />
                  </div>
                  <div className="w-[calc(100%-4rem)] md:w-[calc(50%-2.5rem)] card p-4">
                    <div className="flex flex-col gap-1">
                      <div className="flex justify-between items-center mb-1">
                        <StatusBadge status={h.statusNovo} />
                        <span className="text-xs text-slate-500 font-mono">
                          {new Date(h.dataAlteracao).toLocaleDateString('pt-BR')}
                        </span>
                      </div>
                      <p className="text-xs text-slate-400 mt-2">
                        De: <span className="font-semibold text-slate-300">{h.statusAnterior || 'Nenhum'}</span>
                      </p>
                      {h.motivo && (
                        <p className="text-xs text-slate-400 mt-1 italic border-l-2 border-surface-700 pl-2">
                          "{h.motivo}"
                        </p>
                      )}
                      <p className="text-[10px] text-slate-500 mt-2 text-right">
                        por {h.usuario}
                      </p>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>

      <AtualizarStatusModal
        isOpen={modalOpen}
        onClose={() => setModalOpen(false)}
        sinistroId={sinistro.id}
        novoStatus={statusAlvo}
      />
    </div>
  );
}
