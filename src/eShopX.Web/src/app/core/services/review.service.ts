import { Injectable } from '@angular/core';

import { ApiService } from './api.service';
import {
  CreateReviewRequest,
  CreateReviewResponse,
  GetProductReviewsResponse,
  HomepageReviewItem,
  UpdateReviewRequest,
  UpdateReviewResponse,
} from '../models/api-models';

@Injectable({ providedIn: 'root' })
export class ReviewService {
  constructor(private readonly api: ApiService) {}

  getHomepageReviews(limit = 6): Promise<HomepageReviewItem[]> {
    return this.api.get<HomepageReviewItem[]>('/api/homepage/reviews', { limit });
  }

  getProductReviews(productId: string, page = 1, pageSize = 6): Promise<GetProductReviewsResponse> {
    return this.api.get<GetProductReviewsResponse>(`/api/products/${productId}/reviews`, {
      page,
      pageSize,
    });
  }

  createReview(orderId: string, request: CreateReviewRequest): Promise<CreateReviewResponse> {
    return this.api.post<CreateReviewResponse>(`/api/orders/${orderId}/review`, request);
  }

  updateReview(reviewId: string, request: UpdateReviewRequest): Promise<UpdateReviewResponse> {
    return this.api.put<UpdateReviewResponse>(`/api/reviews/${reviewId}`, request);
  }

  deleteReview(reviewId: string): Promise<void> {
    return this.api.delete<void>(`/api/reviews/${reviewId}`);
  }
}
