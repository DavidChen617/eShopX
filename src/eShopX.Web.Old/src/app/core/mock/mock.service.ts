import { Injectable } from '@angular/core';

import {
  GetCartResponse,
  GetMeResponse,
  GetOrderResponse,
  GetProductDetailResponse,
  GetProductImagesResponse,
  GetProductResponse,
  GetUserOrderResponse,
} from '../models/api-models';
import { BannerSlide, CategoryEntry, RecommendProduct } from '../models/home-models';
import {
  mockCart,
  mockFlashSale,
  mockMe,
  mockOrderDetail,
  mockOrders,
  mockProductDetail,
  mockProductImages,
  mockProducts,
  mockBanners,
  mockCategories,
  mockRecommendProducts,
} from './mock-data';
import { FlashSaleData } from '../models/flash-sale-models';

@Injectable({ providedIn: 'root' })
export class MockService {
  private cartState: GetCartResponse = this.clone(mockCart);

  async getProducts(): Promise<GetProductResponse> {
    return this.withDelay(mockProducts);
  }

  async getProductDetail(productId: string): Promise<GetProductDetailResponse> {
    const found = mockProducts.items.find((item) => item.productId === productId);
    const detail = found
      ? {
          ...mockProductDetail,
          productId: found.productId,
          name: found.name,
          description: found.description,
          price: found.price,
          stockQuantity: found.stockQuantity,
          isActive: found.isActive,
        }
      : mockProductDetail;

    return this.withDelay(detail);
  }

  async getProductImages(): Promise<GetProductImagesResponse> {
    return this.withDelay(mockProductImages);
  }

  async getCart(): Promise<GetCartResponse> {
    return this.withDelay(this.clone(this.cartState));
  }

  async addToCart(productId: string, quantity = 1): Promise<GetCartResponse> {
    const product = mockProducts.items.find((item) => item.productId === productId);
    if (!product) return this.withDelay(this.clone(this.cartState));

    const existing = this.cartState.items.find((item) => item.productId === productId);
    if (existing) {
      existing.quantity += quantity;
      existing.subtotal = existing.unitPrice * existing.quantity;
      existing.stockQuantity = product.stockQuantity;
      existing.inStock = existing.stockQuantity >= existing.quantity;
    } else {
      this.cartState.items.push({
        productId: product.productId,
        productName: product.name,
        unitPrice: product.price,
        quantity,
        subtotal: product.price * quantity,
        stockQuantity: product.stockQuantity,
        inStock: product.stockQuantity >= quantity,
      });
    }

    this.recalculateCart();
    return this.withDelay(this.clone(this.cartState));
  }

  async getOrders(): Promise<GetUserOrderResponse> {
    return this.withDelay(mockOrders);
  }

  async getOrderDetail(orderId: string): Promise<GetOrderResponse> {
    if (mockOrderDetail.orderId === orderId) {
      return this.withDelay(mockOrderDetail);
    }

    return this.withDelay({ ...mockOrderDetail, orderId });
  }

  async getMe(): Promise<GetMeResponse> {
    return this.withDelay(mockMe);
  }

  async getBanners(): Promise<BannerSlide[]> {
    return this.withDelay(mockBanners);
  }

  async getCategories(): Promise<CategoryEntry[]> {
    return this.withDelay(mockCategories);
  }

  async getRecommendProducts(): Promise<RecommendProduct[]> {
    return this.withDelay(mockRecommendProducts);
  }

  async getFlashSale(): Promise<FlashSaleData> {
    return this.withDelay(mockFlashSale);
  }

  private withDelay<T>(value: T, delayMs = 200): Promise<T> {
    return new Promise((resolve) => setTimeout(() => resolve(value), delayMs));
  }

  private recalculateCart(): void {
    this.cartState.totalItems = this.cartState.items.reduce((sum, item) => sum + item.quantity, 0);
    this.cartState.totalAmount = this.cartState.items.reduce((sum, item) => sum + item.subtotal, 0);
  }

  private clone<T>(value: T): T {
    return JSON.parse(JSON.stringify(value)) as T;
  }
}
