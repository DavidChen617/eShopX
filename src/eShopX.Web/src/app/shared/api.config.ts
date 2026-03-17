import { environment } from '../../environments/environment';

export const API_BASE_URL = environment.apiBaseUrl.replace(/\/$/, '');
export const API_V1_BASE_URL = `${API_BASE_URL}/v1`;

export function apiUrl(path: string): string {
  return `${API_BASE_URL}${path.startsWith('/') ? path : `/${path}`}`;
}
