import { useState } from 'react';
import { Modal } from '../ui/Modal';
import { ErrorAlert, LoadingSpinner } from '../ui/Feedback';
import { useAbrirSinistro } from '../../hooks';
import { useApolices } from '../../hooks';
import { getFriendlyError } from '../../api/client';

interface AbrirSinistroModalProps {
  isOpen: boolean;
  onClose: () => void;
}

const hoje = new Date().toISOString().split('T')[0];

export function AbrirSinistroModal({ isOpen, onClose }: AbrirSinistroModalProps) {
  const { data: apolicesData, isLoading: loadingApolices } = useApolices({ status: 'Ativa', pageSize: 100 });
  const mutation = useAbrirSinistro();

  const [form, setForm] = useState({
    apoliceId: '',
    dataOcorrencia: '',
    descricao: '',
    valorEstimado: '',
  });

  const [error, setError] = useState('');

  function handleChange(e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) {
    setForm(f => ({ ...f, [e.target.name]: e.target.value }));
    setError('');
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError('');
    try {
      await mutation.mutateAsync({
        apoliceId:      form.apoliceId,
        dataOcorrencia: new Date(form.dataOcorrencia).toISOString(),
        descricao:      form.descricao,
        valorEstimado:  parseFloat(form.valorEstimado),
        usuario:        'operador',
      });
      setForm({ apoliceId: '', dataOcorrencia: '', descricao: '', valorEstimado: '' });
      onClose();
    } catch (err) {
      setError(getFriendlyError(err));
    }
  }

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Abrir Novo Sinistro" maxWidth="lg">
      {loadingApolices ? (
        <LoadingSpinner />
      ) : (
        <form onSubmit={handleSubmit} className="space-y-4">
          {/* Apólice */}
          <div>
            <label className="label">Apólice Ativa *</label>
            <select
              id="apoliceId"
              name="apoliceId"
              className="select"
              required
              value={form.apoliceId}
              onChange={handleChange}
            >
              <option value="">Selecione uma apólice ativa...</option>
              {apolicesData?.items.map(ap => (
                <option key={ap.id} value={ap.id}>
                  {ap.numero} — {ap.ramo} (válida até {new Date(ap.vigenciaFim).toLocaleDateString('pt-BR')})
                </option>
              ))}
            </select>
            {apolicesData?.items.length === 0 && (
              <p className="mt-1 text-xs text-amber-400">Nenhuma apólice ativa encontrada.</p>
            )}
          </div>

          {/* Data de ocorrência */}
          <div>
            <label className="label">Data de Ocorrência *</label>
            <input
              id="dataOcorrencia"
              name="dataOcorrencia"
              type="date"
              className="input"
              required
              max={hoje}
              value={form.dataOcorrencia}
              onChange={handleChange}
            />
          </div>

          {/* Descrição */}
          <div>
            <label className="label">Descrição *</label>
            <textarea
              id="descricao"
              name="descricao"
              className="input resize-none"
              rows={3}
              required
              minLength={10}
              placeholder="Descreva o sinistro com detalhes..."
              value={form.descricao}
              onChange={handleChange}
            />
          </div>

          {/* Valor estimado */}
          <div>
            <label className="label">Valor Estimado (R$) *</label>
            <input
              id="valorEstimado"
              name="valorEstimado"
              type="number"
              className="input"
              required
              min="0.01"
              step="0.01"
              placeholder="0,00"
              value={form.valorEstimado}
              onChange={handleChange}
            />
          </div>

          {/* Erro do domínio */}
          {error && <ErrorAlert message={error} />}

          {/* Ações */}
          <div className="flex justify-end gap-3 pt-2">
            <button type="button" className="btn-secondary" onClick={onClose}>
              Cancelar
            </button>
            <button type="submit" className="btn-primary" disabled={mutation.isPending}>
              {mutation.isPending ? 'Abrindo...' : 'Abrir Sinistro'}
            </button>
          </div>
        </form>
      )}
    </Modal>
  );
}
