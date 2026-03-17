import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { catchError, map, Observable, of, tap } from 'rxjs';
import { ApiResponse, Tag } from '../models/api.models';
import { apiUrl } from '../shared/api.config';

@Injectable({
  providedIn: 'root',
})
export class TagService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = apiUrl('/v1');

  tags = signal<Tag[]>([]);

  constructor() {
    this.loadTags().subscribe();
  }

  getTags(): Observable<Tag[]> {
    return this.http.get<ApiResponse<Tag[]>>(`${this.baseUrl}/tags`).pipe(
      map((response) => response.data)
    );
  }

  loadTags(): Observable<Tag[]> {
    return this.getTags().pipe(
      tap((tags) => this.tags.set(tags)),
      catchError(() => {
        this.tags.set([]);
        return of([]);
      })
    );
  }

  getTagName(id: string): string {
    return this.tags().find((tag) => tag.id === id)?.name ?? id;
  }
}
