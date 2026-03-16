import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { catchError, map, Observable, of, tap } from 'rxjs';
import { ApiResponse, Category } from '../models/api.models';

@Injectable({
  providedIn: 'root',
})
export class CategoryService {
  private http = inject(HttpClient);
  private readonly baseUrl = '/api/v1';

  categories = signal<Category[]>([]);

  constructor() {
    this.loadCategories().subscribe();
  }

  getCategories(): Observable<Category[]> {
    return this.http.get<ApiResponse<Category[]>>(`${this.baseUrl}/categories`).pipe(
      map((response) => response.data)
    );
  }

  loadCategories(): Observable<Category[]> {
    return this.getCategories().pipe(
      tap((categories) => this.categories.set(categories)),
      catchError(() => {
        this.categories.set([]);
        return of([]);
      })
    );
  }

  getCategoryName(id: string): string {
    const cat = this.categories().find((c) => c.id === id);
    return cat ? cat.name : '未分類';
  }

  createCategory(name: string): Observable<Category> {
    return this.http.post<ApiResponse<Category>>(`${this.baseUrl}/admin/categories`, { name }).pipe(
      map((response) => response.data),
      tap((category) => this.categories.update((items) => [...items, category]))
    );
  }

  renameCategory(id: string, name: string): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/admin/categories/${id}`, { name }).pipe(
      tap(() =>
        this.categories.update((items) =>
          items.map((item) => (item.id === id ? { ...item, name } : item))
        )
      )
    );
  }

  deleteCategory(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/admin/categories/${id}`).pipe(
      tap(() => this.categories.update((items) => items.filter((item) => item.id !== id)))
    );
  }
}
