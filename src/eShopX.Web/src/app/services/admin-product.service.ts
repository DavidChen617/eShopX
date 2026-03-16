import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { catchError, map, Observable, of, switchMap, tap } from 'rxjs';
import {
  ApiResponse,
  CreateProductResponse,
  GetProductsResponse,
  Product,
  ProductListItemResponse,
  ProductSearchResponse,
  ProductUpsertPayload,
  UploadedProductImage,
} from '../models/api.models';
import { ProductService } from './product.service';

@Injectable({
  providedIn: 'root',
})
export class AdminProductService {
  private http = inject(HttpClient);
  private productService = inject(ProductService);
  private readonly baseUrl = '/api/v1';

  products = signal<ProductListItemResponse[]>([]);
  totalCount = signal(0);
  currentPage = signal(1);
  pageSize = signal(10);
  currentKeyword = signal('');

  loadProducts(page = 1, pageSize = 10): Observable<ProductListItemResponse[]> {
    this.currentKeyword.set('');
    const params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);

    return this.http.get<ApiResponse<GetProductsResponse>>(`${this.baseUrl}/products`, { params }).pipe(
      tap((response) => {
        this.products.set(response.data.items);
        this.totalCount.set(response.data.totalCount);
        this.currentPage.set(response.data.page);
        this.pageSize.set(response.data.pageSize);
      }),
      map((response) => response.data.items),
      catchError(() => {
        this.products.set([]);
        this.totalCount.set(0);
        return of([]);
      })
    );
  }

  searchProducts(keyword: string, page = 1, pageSize = 10): Observable<ProductListItemResponse[]> {
    const normalizedKeyword = keyword.trim();
    this.currentKeyword.set(normalizedKeyword);

    return this.productService.search({
      keyword: normalizedKeyword,
      page,
      pageSize,
    }).pipe(
      tap((response: ProductSearchResponse) => {
        this.products.set(
          response.items.map((item) => ({
            id: item.productId,
            name: item.name,
            audience: null,
            isActive: item.isActive,
            categoryId: item.categoryId,
            primaryImageUrl: item.primaryImageUrl,
            startingPrice: item.price,
            colors: [],
          }))
        );
        this.totalCount.set(response.totalCount);
        this.currentPage.set(response.page);
        this.pageSize.set(response.pageSize);
      }),
      map((response) =>
        response.items.map((item) => ({
          id: item.productId,
          name: item.name,
          audience: null,
          isActive: item.isActive,
          categoryId: item.categoryId,
          primaryImageUrl: item.primaryImageUrl,
          startingPrice: item.price,
          colors: [],
        }))
      ),
      catchError(() => {
        this.products.set([]);
        this.totalCount.set(0);
        return of([]);
      })
    );
  }

  getById(id: string): Observable<Product> {
    return this.http.get<ApiResponse<Product>>(`${this.baseUrl}/products/${id}`).pipe(
      map((response) => response.data)
    );
  }

  create(payload: ProductUpsertPayload): Observable<Product> {
    return this.http.post<ApiResponse<CreateProductResponse>>(`${this.baseUrl}/admin/products`, payload).pipe(
      map((response) => response.data),
      switchMap((created) => this.getById(created.productId)),
      tap(() => this.reloadCurrentView().subscribe())
    );
  }

  update(id: string, payload: ProductUpsertPayload): Observable<Product> {
    return this.http.put<void>(`${this.baseUrl}/admin/products/${id}`, payload).pipe(
      switchMap(() => this.getById(id)),
      tap(() => this.reloadCurrentView().subscribe())
    );
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/admin/products/${id}`).pipe(
      tap(() => this.products.update((items) => items.filter((item) => item.id !== id)))
    );
  }

  publish(id: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/admin/products/${id}/publish`, {}).pipe(
      tap(() => this.patchProductStatus(id, true))
    );
  }

  unpublish(id: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/admin/products/${id}/unpublish`, {}).pipe(
      tap(() => this.patchProductStatus(id, false))
    );
  }

  uploadImages(files: File[]): Observable<UploadedProductImage[]> {
    const formData = new FormData();
    files.forEach((file) => formData.append('files', file, file.name));

    return this.http.post<ApiResponse<UploadedProductImage[]> | UploadedProductImage[]>(
      `${this.baseUrl}/admin/products/images`,
      formData
    ).pipe(
      map((response) => Array.isArray(response) ? response : response.data)
    );
  }

  private patchProductStatus(id: string, isActive: boolean) {
    this.products.update((items) =>
      items.map((item) => (item.id === id ? { ...item, isActive } : item))
    );
  }

  private reloadCurrentView(): Observable<ProductListItemResponse[]> {
    const keyword = this.currentKeyword();
    if (keyword) {
      return this.searchProducts(keyword, this.currentPage(), this.pageSize());
    }

    return this.loadProducts(this.currentPage(), this.pageSize());
  }
}
