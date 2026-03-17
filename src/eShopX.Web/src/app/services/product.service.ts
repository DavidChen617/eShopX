import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { map, Observable } from 'rxjs';
import { ApiResponse, Audience, Product, ProductSearchResponse, ProductSummary } from '../models/api.models';
import { apiUrl } from '../shared/api.config';

@Injectable({
  providedIn: 'root',
})
export class ProductService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = apiUrl('/v1/products');

  search(params?: {
    keyword?: string;
    categoryId?: string;
    audience?: Audience;
    minPrice?: number;
    maxPrice?: number;
    isActive?: boolean;
    page?: number;
    pageSize?: number;
  }): Observable<ProductSearchResponse> {
    let httpParams = new HttpParams();

    if (params?.keyword) {
      httpParams = httpParams.set('keyword', params.keyword);
    }

    if (params?.categoryId) {
      httpParams = httpParams.set('categoryId', params.categoryId);
    }

    if (params?.audience) {
      httpParams = httpParams.set('audience', params.audience);
    }

    if (params?.minPrice !== undefined) {
      httpParams = httpParams.set('minPrice', params.minPrice);
    }

    if (params?.maxPrice !== undefined) {
      httpParams = httpParams.set('maxPrice', params.maxPrice);
    }

    if (params?.isActive !== undefined) {
      httpParams = httpParams.set('isActive', params.isActive);
    }

    if (params?.page !== undefined) {
      httpParams = httpParams.set('page', params.page);
    }

    if (params?.pageSize !== undefined) {
      httpParams = httpParams.set('pageSize', params.pageSize);
    }

    return this.http
      .get<ApiResponse<ProductSearchResponse>>(`${this.baseUrl}/search`, { params: httpParams })
      .pipe(map((response) => response.data));
  }

  getFeaturedProducts(pageSize = 8): Observable<ProductSummary[]> {
    return this.search({
      isActive: true,
      page: 1,
      pageSize,
    }).pipe(map((response) => response.items));
  }

  searchEntries(keyword: string, pageSize = 6): Observable<ProductSummary[]> {
    return this.search({
      keyword: keyword.trim(),
      isActive: true,
      page: 1,
      pageSize,
    }).pipe(map((response) => response.items));
  }

  getById(productId: string): Observable<Product> {
    return this.http
      .get<ApiResponse<Product>>(`${this.baseUrl}/${productId}`)
      .pipe(map((response) => response.data));
  }
}
