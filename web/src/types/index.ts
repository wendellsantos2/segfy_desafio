// ─── Enums ────────────────────────────────────────────────────────────────────

export type StatusSinistro = 'Aberto' | 'EmAnalise' | 'Aprovado' | 'Negado' | 'Encerrado';
export type StatusApolice  = 'Ativa' | 'Suspensa' | 'Cancelada';
export type Ramo = 'Auto' | 'Vida' | 'Residencial' | 'Saude' | 'Empresarial';

// ─── Histórico ────────────────────────────────────────────────────────────────

export interface HistoricoSinistro {
  id: string;
  statusAnterior: StatusSinistro | null;
  statusNovo: StatusSinistro;
  dataAlteracao: string; // ISO 8601
  motivo: string | null;
  usuario: string;
}

// ─── Sinistro ────────────────────────────────────────────────────────────────

export interface SinistroResponse {
  id: string;
  apoliceId: string;
  dataOcorrencia: string;
  dataAbertura: string;
  descricao: string;
  valorEstimado: number;
  valorAprovado: number | null;
  motivoNegativa: string | null;
  status: StatusSinistro;
  dataEncerramento: string | null;
  historico: HistoricoSinistro[];
}

// ─── Apólice ─────────────────────────────────────────────────────────────────

export interface ApoliceResponse {
  id: string;
  numero: string;
  clienteId: string;
  ramo: Ramo;
  status: StatusApolice;
  vigenciaInicio: string;
  vigenciaFim: string;
}

// ─── Paginação ────────────────────────────────────────────────────────────────

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
  hasNext: boolean;
  hasPrevious: boolean;
}

// ─── Requests ────────────────────────────────────────────────────────────────

export interface AbrirSinistroRequest {
  apoliceId: string;
  dataOcorrencia: string;
  descricao: string;
  valorEstimado: number;
  usuario?: string;
}

export interface AtualizarStatusRequest {
  status: StatusSinistro;
  usuario: string;
  motivoNegativa?: string;
  valorAprovado?: number;
}

export interface CriarApoliceRequest {
  numero: string;
  clienteId: string;
  ramo: Ramo;
  vigenciaInicio: string;
  vigenciaFim: string;
}

export interface AtualizarStatusApoliceRequest {
  status: 'Suspensa' | 'Ativa' | 'Cancelada';
}

// ─── Filtros ─────────────────────────────────────────────────────────────────

export interface FiltrosSinistro {
  status?: StatusSinistro;
  dataInicio?: string;
  dataFim?: string;
  campoData?: 'abertura' | 'ocorrencia';
  page?: number;
  pageSize?: number;
}

export interface FiltrosApolice {
  status?: StatusApolice;
  page?: number;
  pageSize?: number;
}

// ─── Helpers de máquina de estados ───────────────────────────────────────────

export const TRANSICOES_VALIDAS: Record<StatusSinistro, StatusSinistro[]> = {
  Aberto:    ['EmAnalise', 'Negado'],
  EmAnalise: ['Aprovado', 'Negado'],
  Aprovado:  ['Encerrado'],
  Negado:    [],
  Encerrado: [],
};
