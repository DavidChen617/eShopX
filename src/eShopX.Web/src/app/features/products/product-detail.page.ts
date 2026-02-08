import { Component, signal } from '@angular/core';

import { ActivatedRoute } from '@angular/router';

import { CartService } from '../../core/services/cart.service';
import { AuthService } from '../../core/services/auth.service';
import { ProductService } from '../../core/services/product.service';
import { ReviewService } from '../../core/services/review.service';
import { GetProductDetailResponse, ReviewItem } from '../../core/models/api-models';
import { SectionComponent } from '../../shared/layout/section/section.component';
import { BadgeComponent } from '../../shared/ui/badge/badge.component';
import { ButtonComponent } from '../../shared/ui/button/button.component';
import { ProductGalleryComponent } from '../../shared/ui/product-gallery/product-gallery.component';

@Component({
  selector: 'app-product-detail-page',
  standalone: true,
  imports: [SectionComponent, BadgeComponent, ButtonComponent, ProductGalleryComponent],
  templateUrl: './product-detail.page.html',
})
export class ProductDetailPageComponent {
  product = signal<GetProductDetailResponse | null>(null);
  isLoading = signal(true);
  isAdding = signal(false);
  addMessage = signal('');
  reviews = signal<ReviewItem[]>([]);
  reviewTotal = signal(0);
  reviewAverage = signal(0);
  isReviewLoading = signal(false);

  constructor(
    private readonly route: ActivatedRoute,
    private readonly productService: ProductService,
    private readonly cartService: CartService,
    private readonly authService: AuthService,
    private readonly reviewService: ReviewService,
  ) {
    const productId = this.route.snapshot.paramMap.get('id') ?? '';
    void this.load(productId);
  }

  private async load(productId: string): Promise<void> {
    this.product.set(await this.productService.getProductDetail(productId));
    await this.loadReviews(productId);
    this.isLoading.set(false);
  }

  private async loadReviews(productId: string): Promise<void> {
    this.isReviewLoading.set(true);
    const response = await this.reviewService.getProductReviews(productId, 1, 6);
    this.reviews.set(response.items);
    this.reviewTotal.set(response.totalCount);
    this.reviewAverage.set(Number(response.averageRating.toFixed(1)));
    this.isReviewLoading.set(false);
  }

  async addToCart(): Promise<void> {
    const product = this.product();
    if (!product) return;
    const userId = this.authService.getUserId();
    if (!userId) {
      this.addMessage.set('Please sign in to add to cart');
      return;
    }
    this.isAdding.set(true);
    this.addMessage.set('');
    await this.cartService.addToCart(userId, product.productId, 1);
    this.isAdding.set(false);
    this.addMessage.set('Added to cart');
  }
}
