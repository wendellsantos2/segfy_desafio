import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '../api/client';
import type {
  ApoliceResponse,
  PagedResult,
  FiltrosApolice,
  CriarApoliceRequest,
  AtualizarStatusApoliceRequest,
} from '../types';

// ─── Query keys ──────────────────────────────────────────────────────────────

export const apoliceKeys = {
  all:   ['apolices'] as const,
  lists: () => [...apoliceKeys.all, 'list'] as const,
  list:  (f: FiltrosApolice) => [...apoliceKeys.lists(), f] as const,
};

// ─── useApolices ──────────────────────────────────────────────────────────────

export function useApolices(filtros: FiltrosApolice = {}) {
  return useQuery({
    queryKey: apoliceKeys.list(filtros),
    queryFn: async () => {
      const params = new URLSearchParams();
      if (filtros.status) params.set('status', filtros.status);
      params.set('page',     String(filtros.page     ?? 1));
      params.set('pageSize', String(filtros.pageSize ?? 10));
      const { data } = await api.get<PagedResult<ApoliceResponse>>(`/api/apolices?${params}`);
      return data;
    },
    staleTime: 30_000,
  });
}

// ─── useCriarApolice ──────────────────────────────────────────────────────────

export function useCriarApolice() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (req: CriarApoliceRequest) => {
      const { data } = await api.post<ApoliceResponse>('/api/apolices', req);
      return data;
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: apoliceKeys.lists() });
    },
  });
}

// ─── useAtualizarStatusApolice ────────────────────────────────────────────────

export function useAtualizarStatusApolice(id: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (req: AtualizarStatusApoliceRequest) => {
      await api.patch(`/api/apolices/${id}/status`, req);
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: apoliceKeys.lists() });
    },
  });
}
