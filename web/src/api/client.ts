import axios from 'axios';

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5113',
  headers: { 'Content-Type': 'application/json' },
  timeout: 15_000,
});

// Interceptor: extrai a mensagem de ProblemDetails (RFC 7807) para erros legíveis
api.interceptors.response.use(
  (res) => res,
  (error) => {
    const data = error?.response?.data;
    if (data?.detail) {
      error.friendlyMessage = data.detail;
    } else if (data?.errors) {
      // ValidationException — lista de erros do FluentValidation
      const msgs = Object.values(data.errors as Record<string, string[]>)
        .flat()
        .join(' | ');
      error.friendlyMessage = msgs;
    } else if (data?.title) {
      error.friendlyMessage = data.title;
    }
    return Promise.reject(error);
  }
);

export function getFriendlyError(error: unknown): string {
  if ((error as any)?.friendlyMessage) return (error as any).friendlyMessage;
  if ((error as any)?.message) return (error as any).message;
  return 'Ocorreu um erro inesperado.';
}
