import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

import { ApiService } from './api.service';
import {
  CreateProductRequest,
  CreateProductResponse,
  GetProductResponse,
  UpdateProductRequest,
  GetProductImagesResponse,
  UploadProductImageResponse,
  UpdateProductImageResponse,
} from '../models/api-models';
import { ApiResponse } from './api.service';

@Injectable({ providedIn: 'root' })
export class SellerProductService {
  constructor(
    private readonly api: ApiService,
    private readonly http: HttpClient
  ) {}

  getMyProducts(params?: {
    keyword?: string;
    isActive?: boolean;
    minPrice?: number;
    maxPrice?: number;
    page?: number;
    pageSize?: number;
  }): Promise<GetProductResponse> {
    return this.api.get<GetProductResponse>('/api/products/mine', params);
  }

  create(request: CreateProductRequest): Promise<CreateProductResponse> {
    return this.api.post<CreateProductResponse, CreateProductRequest>('/api/products', request);
  }

  update(productId: string, request: UpdateProductRequest): Promise<void> {
    return this.api.put<void, UpdateProductRequest>(`/api/products/${productId}`, request);
  }

  delete(productId: string): Promise<void> {
    return this.api.delete<void>(`/api/products/${productId}`);
  }

  getImages(productId: string): Promise<GetProductImagesResponse> {
    return this.api.get<GetProductImagesResponse>(`/api/products/${productId}/images`);
  }

  async uploadImage(
    productId: string,
    file: File,
    isPrimary = false,
    sortOrder = 0
  ): Promise<UploadProductImageResponse> {
    const form = new FormData();
    form.append('file', file);
    form.append('isPrimary', String(isPrimary));
    form.append('sortOrder', String(sortOrder));

    const response = await firstValueFrom(
      this.http.post<ApiResponse<UploadProductImageResponse>>(`/api/products/${productId}/images`, form)
    );
    if (response.isError) {
      throw new Error(response.message || 'Upload failed');
    }
    return (response.result ?? null) as UploadProductImageResponse;
  }

  async updateImage(
    productId: string,
    imageId: string,
    payload: { isPrimary?: boolean; sortOrder?: number }
  ): Promise<UpdateProductImageResponse> {
    const response = await firstValueFrom(
      this.http.put<ApiResponse<UpdateProductImageResponse>>(
        `/api/products/${productId}/images/${imageId}`,
        payload
      )
    );
    if (response.isError) {
      throw new Error(response.message || 'Update failed');
    }
    return (response.result ?? null) as UpdateProductImageResponse;
  }

  async deleteImage(productId: string, imageId: string): Promise<void> {
    const response = await firstValueFrom(
      this.http.delete<ApiResponse<null>>(`/api/products/${productId}/images/${imageId}`)
    );
    if (response.isError) {
      throw new Error(response.message || 'Delete failed');
    }
  }
}
