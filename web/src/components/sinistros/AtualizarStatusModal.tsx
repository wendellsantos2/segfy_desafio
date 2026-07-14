import { useState } from 'react';
import { Modal } from '../ui/Modal';
import { ErrorAlert } from '../ui/Feedback';
import { useAtualizarStatus } from '../../hooks';
import { getFriendlyError } from '../../api/client';
import type { StatusSinistro } from '../../types';

const STATUS_LABELS: Record<StatusSinistro, string> = {
  Aberto:    'Aberto',
  EmAnalise: 'Em Análise',
  Aprovado:  'Aprovado',
  Negado:    'Negado',
  Encerrado: 'Encerrado',
};

interface AtualizarStatusModalProps {
  sinistroId: string;
  novoStatus: StatusSinistro;
  isOpen: boolean;
  onClose: () => void;
}

export function AtualizarStatusModal({ sinistroId, novoStatus, isOpen, onClose }: AtualizarStatusModalProps) {
  const mutation = useAtualizarStatus(sinistroId);
  const [motivo, setMotivo] = useState('');
  const [valor, setValor] = useState('');
  const [error, setError] = useState('');

  const precisaMotivo = novoStatus === 'Negado';
  const precisaValor  = novoStatus === 'Encerrado';

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError('');
    try {
      await mutation.mutateAsync({
        status:          novoStatus,
        usuario:         'operador',
        motivoNegativa:  precisaMotivo ? motivo : undefined,
        valorAprovado:   precisaValor  ? parseFloat(valor) : undefined,
      });
      setMotivo('');
      setValor('');
      onClose();
    } catch (err) {
      setError(getFriendlyError(err));
    }
  }

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title={`Mover para: ${STATUS_LABELS[novoStatus]}`}
    >
      <form onSubmit={handleSubmit} className="space-y-4">
        {precisaMotivo && (
          <div>
            <label className="label">Motivo da Negativa * <span className="text-slate-600">(mín. 10 caracteres)</span></label>
            <textarea
              id="motivo"
              className="input resize-none"
              rows={3}
              required
              minLength={10}
              placeholder="Descreva o motivo da negativa..."
              value={motivo}
              onChange={e => { setMotivo(e.target.value); setError(''); }}
            />
          </div>
        )}

        {precisaValor && (
          <div>
            <label className="label">Valor Aprovado (R$) *</label>
            <input
              id="valorAprovado"
              type="number"
              className="input"
              required
              min="0.01"
              step="0.01"
              placeholder="0,00"
              value={valor}
              onChange={e => { setValor(e.target.value); setError(''); }}
            />
          </div>
        )}

        {!precisaMotivo && !precisaValor && (
          <p className="text-sm text-slate-400">
            Confirma a transição para <strong className="text-slate-200">{STATUS_LABELS[novoStatus]}</strong>?
          </p>
        )}

        {error && <ErrorAlert message={error} />}

        <div className="flex justify-end gap-3 pt-2">
          <button type="button" className="btn-secondary" onClick={onClose}>
            Cancelar
          </button>
          <button type="submit" className="btn-primary" disabled={mutation.isPending}>
            {mutation.isPending ? 'Atualizando...' : 'Confirmar'}
          </button>
        </div>
      </form>
    </Modal>
  );
}
