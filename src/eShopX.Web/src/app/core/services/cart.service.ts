import { Injectable, signal } from '@angular/core';

import { ApiService } from './api.service';
import { GetCartResponse, UpdateCartItemResponse } from '../models/api-models';

export interface AddCartItemRequest {
  productId: string;
  quantity: number;
}

export interface AddCartItemResponse {
  cartId: string;
  productId: string;
  quantity: number;
  updatedAt: string;
}

@Injectable({ providedIn: 'root' })
export class CartService {
  readonly cartCount = signal(0);
  constructor(private readonly api: ApiService) {}

  async getCart(userId: string): Promise<GetCartResponse> {
    const cart = await this.api.get<GetCartResponse>(`/api/carts/${userId}`);
    this.cartCount.set(cart.totalItems ?? 0);
    return cart;
  }

  async addToCart(userId: string, productId: string, quantity = 1): Promise<AddCartItemResponse> {
    const result = await this.api.post<AddCartItemResponse, AddCartItemRequest>(
      `/api/carts/${userId}/items`,
      { productId, quantity }
    );
    await this.getCart(userId);
    return result;
  }

  async updateQuantity(userId: string, productId: string, quantity: number): Promise<UpdateCartItemResponse> {
    const result = await this.api.put<UpdateCartItemResponse, { quantity: number }>(
      `/api/carts/${userId}/items/${productId}`,
      { quantity }
    );
    await this.getCart(userId);
    return result;
  }

  async removeItem(userId: string, productId: string): Promise<void> {
    await this.api.delete<void>(`/api/carts/${userId}/items/${productId}`);
    await this.getCart(userId);
  }

  async clearCart(userId: string): Promise<void> {
    await this.api.delete<void>(`/api/carts/${userId}/items`);
    this.cartCount.set(0);
  }
}
