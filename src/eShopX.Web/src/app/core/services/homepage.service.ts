import { Injectable } from '@angular/core';

import { ApiService } from './api.service';
import { BannerSlide, CategoryEntry, RecommendProduct } from '../models/home-models';
import { FlashSaleData } from '../models/flash-sale-models';

interface GetBannersResponse {
  items: Array<{
    id: string;
    title: string;
    imageUrl: string;
    link: string;
  }>;
}

interface GetCategoriesResponse {
  items: Array<{
    id: string;
    name: string;
    icon: string;
    link: string;
  }>;
}

interface GetFlashSaleResponse {
  id: string;
  title: string;
  subtitle: string | null;
  startsAt: string;
  endsAt: string;
  slots: Array<{
    id: string;
    label: string;
    startsAt: string;
    endsAt: string;
    status: string;
  }>;
  items: Array<{
    productId: string;
    name: string;
    imageUrl: string | null;
    price: number;
    originalPrice: number;
    stockTotal: number;
    stockRemaining: number;
    badge: string | null;
  }>;
}

interface GetRecommendProductsResponse {
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  items: Array<{
    id: string;
    name: string;
    imageUrl: string | null;
    price: number;
    originalPrice: number | null;
    salesCount: number;
  }>;
}

@Injectable({ providedIn: 'root' })
export class HomepageService {
  constructor(private readonly api: ApiService) {}

  async getBanners(): Promise<BannerSlide[]> {
    const response = await this.api.get<GetBannersResponse>('/api/homepage/banners');
    return response.items.map(item => ({
      id: item.id,
      title: item.title,
      imageUrl: item.imageUrl,
      link: item.link,
    }));
  }

  async getCategories(): Promise<CategoryEntry[]> {
    const response = await this.api.get<GetCategoriesResponse>('/api/homepage/categories');
    return response.items.map(item => ({
      id: item.id,
      name: item.name,
      icon: item.icon,
      link: item.link,
    }));
  }

  async getFlashSale(): Promise<FlashSaleData | null> {
    const response = await this.api.get<GetFlashSaleResponse | null>('/api/homepage/flash-sale');
    if (!response) return null;

    return {
      title: response.title,
      subtitle: response.subtitle ?? '',
      startsAt: response.startsAt,
      endsAt: response.endsAt,
      slots: response.slots.map(slot => ({
        id: slot.id,
        label: slot.label,
        startsAt: slot.startsAt,
        status: slot.status as 'upcoming' | 'live' | 'ended',
      })),
      items: response.items.map(item => ({
        productId: item.productId,
        name: item.name,
        imageUrl: item.imageUrl ?? 'https://placehold.co/320x320',
        price: item.price,
        originalPrice: item.originalPrice,
        stockTotal: item.stockTotal,
        stockRemaining: item.stockRemaining,
        badge: item.badge ?? undefined,
      })),
      rules: [
        '需登入會員才能參與秒殺。',
        '商品售完即止，付款未完成將釋放庫存。',
        '每位會員每商品限購 1 件。',
      ],
    };
  }

  async getRecommendProducts(): Promise<RecommendProduct[]> {
    const response = await this.api.get<GetRecommendProductsResponse>('/api/homepage/recommend');
    return response.items.map(item => ({
      id: item.id,
      name: item.name,
      imageUrl: item.imageUrl ?? 'https://placehold.co/400x400',
      price: item.price,
      originalPrice: item.originalPrice ?? undefined,
      salesCount: item.salesCount,
    }));
  }
}
