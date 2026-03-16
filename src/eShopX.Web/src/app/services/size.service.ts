import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { catchError, map, Observable, of, tap } from 'rxjs';
import { ApiResponse, Size } from '../models/api.models';

@Injectable({
  providedIn: 'root',
})
export class SizeService {
  private http = inject(HttpClient);
  private readonly baseUrl = '/api/v1';

  sizes = signal<Size[]>([]);

  constructor() {
    this.loadSizes().subscribe();
  }

  getSizes(type?: string): Observable<Size[]> {
    const params = type ? new HttpParams().set('type', type) : undefined;
    return this.http.get<ApiResponse<Size[]>>(`${this.baseUrl}/sizes`, { params }).pipe(
      map((response) => response.data)
    );
  }

  loadSizes(type?: string): Observable<Size[]> {
    return this.getSizes(type).pipe(
      tap((sizes) => this.sizes.set(sizes)),
      catchError(() => {
        this.sizes.set([]);
        return of([]);
      })
    );
  }

  getSizeName(id: string): string {
    return this.sizes().find((size) => size.id === id)?.name ?? id;
  }

  createSize(name: string, type: string): Observable<Size> {
    return this.http.post<ApiResponse<Size>>(`${this.baseUrl}/admin/sizes`, { name, type }).pipe(
      map((response) => response.data),
      tap((size) => this.sizes.update((items) => [...items, size]))
    );
  }

  renameSize(id: string, name: string): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/admin/sizes/${id}`, { name }).pipe(
      tap(() =>
        this.sizes.update((items) =>
          items.map((item) => (item.id === id ? { ...item, name } : item))
        )
      )
    );
  }

  deleteSize(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/admin/sizes/${id}`).pipe(
      tap(() => this.sizes.update((items) => items.filter((item) => item.id !== id)))
    );
  }
}
