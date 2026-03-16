import { Injectable } from '@angular/core';

import { ApiService } from './api.service';
import { GetProductDetailResponse, GetProductResponse } from '../models/api-models';

@Injectable({ providedIn: 'root' })
export class ProductService {
  constructor(private readonly api: ApiService) {}

  getProducts(params?: {
    keyword?: string;
    isActive?: boolean;
    minPrice?: number;
    maxPrice?: number;
    categoryId?: string;
    page?: number;
    pageSize?: number;
  }): Promise<GetProductResponse> {
    return this.api.get<GetProductResponse>('/api/products', params);
  }

  searchProducts(keyword: string): Promise<GetProductResponse> {
    return this.getProducts({ keyword: keyword.trim() || undefined });
  }

  getProductDetail(productId: string): Promise<GetProductDetailResponse> {
    return this.api.get<GetProductDetailResponse>(`/api/products/${productId}`);
  }
}
