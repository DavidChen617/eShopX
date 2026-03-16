import { Component, signal } from '@angular/core';
import { NzSpinModule } from 'ng-zorro-antd/spin';

import { HomepageService } from '../../core/services/homepage.service';
import { ReviewService } from '../../core/services/review.service';
import { BannerSlide, CategoryEntry, RecommendProduct } from '../../core/models/home-models';
import { HomepageReviewItem } from '../../core/models/api-models';

import { BannerCarouselComponent } from './components/banner-carousel/banner-carousel.component';
import { CategoryGridComponent } from './components/category-grid/category-grid.component';
import { ProductFeedComponent } from './components/product-feed/product-feed.component';

@Component({
  selector: 'app-home-page',
  standalone: true,
  imports: [
    NzSpinModule,
    BannerCarouselComponent,
    CategoryGridComponent,
    ProductFeedComponent,
  ],
  templateUrl: './home.page.html',
})
export class HomePageComponent {
  banners = signal<BannerSlide[]>([]);
  categories = signal<CategoryEntry[]>([]);
  recommendProducts = signal<RecommendProduct[]>([]);
  homepageReviews = signal<HomepageReviewItem[]>([]);
  isLoading = signal(true);

  constructor(
    private readonly homepageService: HomepageService,
    private readonly reviewService: ReviewService,
  ) {
    void this.load();
  }

  private async load(): Promise<void> {
    try {
      const [bannersResult, categoriesResult, recommendProductsResult, homepageReviewsResult] =
        await Promise.allSettled([
          this.homepageService.getBanners(),
          this.homepageService.getCategories(),
          this.homepageService.getRecommendProducts(),
          this.reviewService.getHomepageReviews(6),
        ]);

      this.banners.set(bannersResult.status === 'fulfilled' ? bannersResult.value : []);
      this.categories.set(categoriesResult.status === 'fulfilled' ? categoriesResult.value : []);
      this.recommendProducts.set(
        recommendProductsResult.status === 'fulfilled' ? recommendProductsResult.value : [],
      );
      this.homepageReviews.set(
        homepageReviewsResult.status === 'fulfilled' ? homepageReviewsResult.value : [],
      );
    } finally {
      this.isLoading.set(false);
    }
  }
}
