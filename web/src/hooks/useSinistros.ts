import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '../api/client';
import type {
  SinistroResponse,
  HistoricoSinistro,
  PagedResult,
  FiltrosSinistro,
  AbrirSinistroRequest,
  AtualizarStatusRequest,
} from '../types';

// ─── Query keys ──────────────────────────────────────────────────────────────

export const sinistroKeys = {
  all:      ['sinistros'] as const,
  lists:    () => [...sinistroKeys.all, 'list'] as const,
  list:     (f: FiltrosSinistro) => [...sinistroKeys.lists(), f] as const,
  details:  () => [...sinistroKeys.all, 'detail'] as const,
  detail:   (id: string) => [...sinistroKeys.details(), id] as const,
  historico:(id: string) => [...sinistroKeys.all, 'historico', id] as const,
};

// ─── useSinistros ─────────────────────────────────────────────────────────────

export function useSinistros(filtros: FiltrosSinistro = {}) {
  return useQuery({
    queryKey: sinistroKeys.list(filtros),
    queryFn: async () => {
      const params = new URLSearchParams();
      if (filtros.status)     params.set('status', filtros.status);
      if (filtros.dataInicio) params.set('dataInicio', filtros.dataInicio);
      if (filtros.dataFim)    params.set('dataFim', filtros.dataFim);
      if (filtros.campoData)  params.set('campoData', filtros.campoData);
      params.set('page',     String(filtros.page     ?? 1));
      params.set('pageSize', String(filtros.pageSize ?? 10));
      const { data } = await api.get<PagedResult<SinistroResponse>>(`/api/sinistros?${params}`);
      return data;
    },
    staleTime: 30_000,
  });
}

// ─── useSinistro ──────────────────────────────────────────────────────────────

export function useSinistro(id: string) {
  return useQuery({
    queryKey: sinistroKeys.detail(id),
    queryFn: async () => {
      const { data } = await api.get<SinistroResponse>(`/api/sinistros/${id}`);
      return data;
    },
    enabled: !!id,
    staleTime: 15_000,
  });
}

// ─── useHistorico ─────────────────────────────────────────────────────────────

export function useHistorico(id: string) {
  return useQuery({
    queryKey: sinistroKeys.historico(id),
    queryFn: async () => {
      const { data } = await api.get<HistoricoSinistro[]>(`/api/sinistros/${id}/historico`);
      return data;
    },
    enabled: !!id,
    staleTime: 10_000,
  });
}

// ─── useAbrirSinistro ─────────────────────────────────────────────────────────

export function useAbrirSinistro() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (req: AbrirSinistroRequest) => {
      const { data } = await api.post<SinistroResponse>('/api/sinistros', req);
      return data;
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: sinistroKeys.lists() });
    },
  });
}

// ─── useAtualizarStatus ───────────────────────────────────────────────────────

export function useAtualizarStatus(id: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (req: AtualizarStatusRequest) => {
      await api.patch(`/api/sinistros/${id}/status`, req);
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: sinistroKeys.detail(id) });
      qc.invalidateQueries({ queryKey: sinistroKeys.historico(id) });
      qc.invalidateQueries({ queryKey: sinistroKeys.lists() });
    },
  });
}
