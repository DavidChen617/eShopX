import { Component, signal } from '@angular/core';
import { NzSpinModule } from 'ng-zorro-antd/spin';

import { HomepageService } from '../../core/services/homepage.service';
import { FlashSaleData } from '../../core/models/flash-sale-models';
import { BannerSlide, CategoryEntry, RecommendProduct } from '../../core/models/home-models';

import { BannerCarouselComponent } from './components/banner-carousel/banner-carousel.component';
import { FlashSaleSectionComponent } from './components/flash-sale-section/flash-sale-section.component';
import { CategoryGridComponent } from './components/category-grid/category-grid.component';
import { ProductFeedComponent } from './components/product-feed/product-feed.component';

@Component({
  selector: 'app-home-page',
  standalone: true,
  imports: [
    NzSpinModule,
    BannerCarouselComponent,
    FlashSaleSectionComponent,
    CategoryGridComponent,
    ProductFeedComponent,
  ],
  templateUrl: './home.page.html',
})
export class HomePageComponent {
  banners = signal<BannerSlide[]>([]);
  categories = signal<CategoryEntry[]>([]);
  flashSale = signal<FlashSaleData | null>(null);
  recommendProducts = signal<RecommendProduct[]>([]);
  isLoading = signal(true);

  constructor(private readonly homepageService: HomepageService) {
    void this.load();
  }

  private async load(): Promise<void> {
    const [banners, categories, flashSale, recommendProducts] = await Promise.all([
      this.homepageService.getBanners(),
      this.homepageService.getCategories(),
      this.homepageService.getFlashSale(),
      this.homepageService.getRecommendProducts(),
    ]);

    this.banners.set(banners);
    this.categories.set(categories);
    this.flashSale.set(flashSale);
    this.recommendProducts.set(recommendProducts);
    this.isLoading.set(false);
  }
}
