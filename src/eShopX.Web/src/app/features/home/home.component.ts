import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ProductCardComponent } from '../../components/product-card.component';
import { ProductSummary } from '../../models/api.models';
import { CategoryService } from '../../services/category.service';
import { ProductService } from '../../services/product.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink, ProductCardComponent],
  template: `
    <div class="max-w-7xl mx-auto px-4 py-8">
      <!-- Hero Section -->
      <div class="mb-12 rounded-3xl bg-indigo-600 p-8 md:p-16 text-white overflow-hidden relative">
        <div class="relative z-10 max-w-lg">
          <span class="inline-block px-3 py-1 rounded-full bg-white/20 text-sm font-medium mb-4 backdrop-blur-sm">{{ seasonalHeadline }}</span>
          <h1 class="text-4xl md:text-6xl font-black mb-6 leading-tight">探索你的<br>極致生活風格</h1>
          <p class="text-lg text-indigo-100 mb-8">精選男裝與女裝單品，兼顧版型、舒適與日常搭配。</p>
        </div>
        <!-- Decorative circles -->
        <div class="absolute top-0 right-0 w-64 h-64 bg-white/10 rounded-full -translate-y-1/2 translate-x-1/2 blur-3xl"></div>
        <div class="absolute bottom-0 right-0 w-96 h-96 bg-indigo-500/50 rounded-full translate-y-1/3 translate-x-1/4 blur-3xl"></div>
      </div>

      <!-- Product Grid Header -->
      <div class="flex items-center justify-between mb-8">
        <h2 class="text-2xl font-bold text-slate-900">熱門精選</h2>
        <a
          routerLink="/products"
          class="text-indigo-600 font-semibold flex items-center gap-1 hover:underline"
        >
          查看全部 <i class="pi pi-arrow-right text-xs"></i>
        </a>
      </div>

      <!-- Grid -->
      @if (isLoading()) {
        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
          @for (item of [1, 2, 3, 4]; track item) {
            <div class="overflow-hidden rounded-2xl bg-white shadow-sm">
              <div class="aspect-square animate-pulse bg-slate-200"></div>
              <div class="space-y-3 p-5">
                <div class="h-3 w-20 animate-pulse rounded bg-slate-200"></div>
                <div class="h-6 w-3/4 animate-pulse rounded bg-slate-200"></div>
                <div class="h-8 w-24 animate-pulse rounded bg-slate-200"></div>
              </div>
            </div>
          }
        </div>
      } @else if (products().length === 0) {
        <div class="rounded-3xl border border-dashed border-slate-200 bg-white px-6 py-16 text-center">
          <p class="text-lg font-bold text-slate-900">目前沒有可顯示的商品</p>
          <p class="mt-2 text-slate-500">等商品上架後，這裡會自動帶出最新內容。</p>
        </div>
      } @else {
        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
          @for (prod of cardProducts(); track prod.id) {
            <app-product-card [product]="prod"></app-product-card>
          }
        </div>
      }
    </div>
  `,
})
export class HomeComponent implements OnInit {
  private readonly productService = inject(ProductService);
  private readonly categoryService = inject(CategoryService);

  protected readonly isLoading = signal(true);
  protected readonly products = signal<ProductSummary[]>([]);
  protected readonly seasonalHeadline = `${this.getCurrentSeason()}季新品上市`;

  protected readonly cardProducts = computed(() =>
    this.products().map((product) => ({
      id: product.productId,
      name: product.name,
      price: product.price,
      image: product.primaryImageUrl,
      category: this.categoryService.getCategoryName(product.categoryId),
      createdAt: '',
    }))
  );

  ngOnInit(): void {
    this.productService.getFeaturedProducts(8).subscribe({
      next: (products) => {
        this.products.set(products);
        this.isLoading.set(false);
      },
      error: () => {
        this.products.set([]);
        this.isLoading.set(false);
      },
    });
  }

  private getCurrentSeason(date = new Date()): '春' | '夏' | '秋' | '冬' {
    const month = date.getMonth() + 1;

    if (month >= 3 && month <= 5) {
      return '春';
    }

    if (month >= 6 && month <= 8) {
      return '夏';
    }

    if (month >= 9 && month <= 11) {
      return '秋';
    }

    return '冬';
  }
}
