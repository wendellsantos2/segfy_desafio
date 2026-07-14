import type { StatusSinistro, StatusApolice } from '../../types';

type BadgeVariant = StatusSinistro | StatusApolice | 'default';

const SINISTRO_STYLES: Record<StatusSinistro, string> = {
  Aberto:    'bg-sky-500/15 text-sky-400 border-sky-500/30',
  EmAnalise: 'bg-amber-500/15 text-amber-400 border-amber-500/30',
  Aprovado:  'bg-emerald-500/15 text-emerald-400 border-emerald-500/30',
  Negado:    'bg-rose-500/15 text-rose-400 border-rose-500/30',
  Encerrado: 'bg-slate-500/15 text-slate-400 border-slate-500/30',
};

const APOLICE_STYLES: Record<StatusApolice, string> = {
  Ativa:     'bg-emerald-500/15 text-emerald-400 border-emerald-500/30',
  Suspensa:  'bg-amber-500/15 text-amber-400 border-amber-500/30',
  Cancelada: 'bg-rose-500/15 text-rose-400 border-rose-500/30',
};

const LABEL_MAP: Record<string, string> = {
  EmAnalise: 'Em Análise',
};

interface BadgeProps {
  status: BadgeVariant;
  className?: string;
}

export function StatusBadge({ status, className = '' }: BadgeProps) {
  const style =
    SINISTRO_STYLES[status as StatusSinistro] ??
    APOLICE_STYLES[status as StatusApolice] ??
    'bg-slate-500/15 text-slate-400 border-slate-500/30';

  const label = LABEL_MAP[status] ?? status;

  return (
    <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium border whitespace-nowrap ${style} ${className}`}>
      {label}
    </span>
  );
}
