import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

export interface ApiResponse<T> {
  result?: T | null;
  message?: string;
  isError: boolean;
  statusCode: number;
}

@Injectable({ providedIn: 'root' })
export class ApiService {
  constructor(private readonly http: HttpClient) {}

  async get<T>(path: string, params?: Record<string, string | number | boolean | undefined>): Promise<T> {
    let httpParams = new HttpParams();
    if (params) {
      Object.entries(params).forEach(([key, value]) => {
        if (value !== undefined && value !== null) {
          httpParams = httpParams.set(key, String(value));
        }
      });
    }

    const response = await firstValueFrom(
      this.http.get<T>(path, { params: httpParams })
    );
    return this.unwrapMaybe(response);
  }

  async post<T, B = unknown>(path: string, body?: B): Promise<T> {
    const response = await firstValueFrom(
      this.http.post<T>(path, body)
    );
    return this.unwrapMaybe(response);
  }

  async put<T, B = unknown>(path: string, body?: B): Promise<T> {
    const response = await firstValueFrom(
      this.http.put<T>(path, body)
    );
    return this.unwrapMaybe(response);
  }

  async delete<T>(path: string): Promise<T> {
    const response = await firstValueFrom(
      this.http.delete<T>(path)
    );
    return this.unwrapMaybe(response);
  }

  private unwrapMaybe<T>(response: T): T {
    if (response && typeof response === 'object' && 'isError' in response) {
      const api = response as unknown as ApiResponse<T>;
      if (api.isError) {
        throw new Error(api.message || 'Request failed');
      }
      return (api.result ?? null) as T;
    }
    return response;
  }
}
