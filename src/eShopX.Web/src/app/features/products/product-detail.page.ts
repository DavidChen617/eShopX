import { Component, signal } from '@angular/core';

import { ActivatedRoute } from '@angular/router';

import { CartService } from '../../core/services/cart.service';
import { AuthService } from '../../core/services/auth.service';
import { ProductService } from '../../core/services/product.service';
import { GetProductDetailResponse } from '../../core/models/api-models';
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

  constructor(
    private readonly route: ActivatedRoute,
    private readonly productService: ProductService,
    private readonly cartService: CartService,
    private readonly authService: AuthService
  ) {
    const productId = this.route.snapshot.paramMap.get('id') ?? '';
    void this.load(productId);
  }

  private async load(productId: string): Promise<void> {
    this.product.set(await this.productService.getProductDetail(productId));
    this.isLoading.set(false);
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
