import { Injectable } from '@angular/core';

import { HomepageService } from './homepage.service';
import { FlashSaleData } from '../models/flash-sale-models';

@Injectable({ providedIn: 'root' })
export class FlashSaleService {
  constructor(private readonly homepageService: HomepageService) {}

  async getFlashSale(): Promise<FlashSaleData | null> {
    return this.homepageService.getFlashSale();
  }
}
