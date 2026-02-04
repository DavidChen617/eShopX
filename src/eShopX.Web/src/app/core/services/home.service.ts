import { Injectable } from '@angular/core';

import { MockService } from '../mock/mock.service';
import { BannerSlide, CategoryEntry, RecommendProduct } from '../models/home-models';
import { FlashSaleData } from '../models/flash-sale-models';

@Injectable({ providedIn: 'root' })
export class HomeService {
  constructor(private readonly mockService: MockService) {}

  getBanners(): Promise<BannerSlide[]> {
    return this.mockService.getBanners();
  }

  getCategories(): Promise<CategoryEntry[]> {
    return this.mockService.getCategories();
  }

  getRecommendProducts(): Promise<RecommendProduct[]> {
    return this.mockService.getRecommendProducts();
  }

  getFlashSale(): Promise<FlashSaleData> {
    return this.mockService.getFlashSale();
  }
}
