import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { Product, ProductImage, ProductVariant, SKU } from '../../models/api.models';
import { CategoryService } from '../../services/category.service';
import { ProductService } from '../../services/product.service';
import { SizeService } from '../../services/size.service';
import { TagService } from '../../services/tag.service';
import { CartService } from '../../services/cart.service';

@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, ButtonModule],
  template: `
    <div class="max-w-7xl mx-auto px-4 py-8 md:py-12">
      @if (isLoading()) {
        <div class="grid grid-cols-1 gap-12 lg:grid-cols-2">
          <div class="aspect-[4/5] animate-pulse rounded-3xl bg-slate-200"></div>
          <div class="space-y-5">
            <div class="h-4 w-40 animate-pulse rounded bg-slate-200"></div>
            <div class="h-10 w-3/4 animate-pulse rounded bg-slate-200"></div>
            <div class="h-8 w-32 animate-pulse rounded bg-slate-200"></div>
            <div class="h-28 w-full animate-pulse rounded bg-slate-200"></div>
            <div class="h-24 w-full animate-pulse rounded bg-slate-200"></div>
          </div>
        </div>
      } @else if (!product()) {
        <div class="rounded-3xl border border-dashed border-slate-200 bg-white px-6 py-16 text-center">
          <p class="text-lg font-bold text-slate-900">找不到商品</p>
          <p class="mt-2 text-slate-500">這個商品可能已下架，或網址有誤。</p>
          <a
            routerLink="/products"
            class="mt-6 inline-flex rounded-full bg-slate-900 px-6 py-3 font-bold text-white"
          >
            返回商品列表
          </a>
        </div>
      } @else {
        <nav class="mb-8 flex w-fit items-center gap-2 rounded-full bg-slate-100/50 px-4 py-2 text-sm text-slate-500">
          <a routerLink="/" class="flex items-center gap-1 transition-colors hover:text-indigo-600">
            <i class="pi pi-home text-[10px]"></i> 首頁
          </a>
          <i class="pi pi-chevron-right text-[8px] text-slate-300"></i>
          <a
            routerLink="/products"
            [queryParams]="audienceQueryParams()"
            class="transition-colors hover:text-indigo-600"
          >
            {{ audienceName() }}
          </a>
          <i class="pi pi-chevron-right text-[8px] text-slate-300"></i>
          <a
            routerLink="/products"
            [queryParams]="categoryQueryParams()"
            class="transition-colors hover:text-indigo-600"
          >
            {{ categoryName() }}
          </a>
          <i class="pi pi-chevron-right text-[8px] text-slate-300"></i>
          <span class="font-bold text-slate-900">{{ product()!.name }}</span>
        </nav>

        <div class="grid grid-cols-1 gap-12 lg:grid-cols-2">
          <div class="space-y-4">
            <div class="overflow-hidden rounded-3xl bg-slate-100 shadow-sm">
              <img
                [src]="activeImage().url"
                [alt]="product()!.name"
                class="w-full aspect-[4/5] object-cover"
              />
            </div>

            @if (activeVariantImages().length > 1) {
              <div class="grid grid-cols-5 gap-3">
                @for (image of activeVariantImages(); track image.id; let idx = $index) {
                  <button
                    type="button"
                    (click)="activeImageIndex.set(idx)"
                    class="overflow-hidden rounded-2xl border-2 bg-slate-100 transition-all"
                    [class.border-indigo-600]="activeImageIndex() === idx"
                    [class.shadow-lg]="activeImageIndex() === idx"
                    [class.border-transparent]="activeImageIndex() !== idx"
                    [attr.aria-label]="'切換到第 ' + (idx + 1) + ' 張圖片'"
                  >
                    <img
                      [src]="image.url"
                      [alt]="product()!.name"
                      class="aspect-square w-full object-cover"
                    />
                  </button>
                }
              </div>
            }
          </div>

          <div class="flex flex-col">
            @if (displayTags().length > 0) {
              <div class="mb-4 flex flex-wrap gap-2">
                @for (tagName of displayTags(); track tagName) {
                  <span class="rounded-full border border-indigo-100 bg-indigo-50 px-3 py-1 text-[10px] font-bold uppercase tracking-widest text-indigo-600">
                    {{ tagName }}
                  </span>
                }
              </div>
            }

            <h1 class="mb-2 text-3xl font-black tracking-tight text-slate-900 md:text-4xl">
              {{ product()!.name }}
            </h1>

            <div class="mb-6 flex items-center gap-4">
              <span class="text-3xl font-black text-indigo-600">
                {{ activeSKU()?.price ?? minPrice() | currency : 'TWD' : 'symbol' : '1.0-0' }}
              </span>
              <span
                class="flex items-center gap-1 rounded-full px-3 py-1 text-xs font-bold"
                [class]="stockBadgeClass()"
              >
                <i class="pi" [class.pi-check-circle]="(activeSKU()?.stock ?? totalStock()) > 0" [class.pi-exclamation-circle]="(activeSKU()?.stock ?? totalStock()) <= 0"></i>
                {{ stockLabel() }}
              </span>
            </div>

            <p class="mb-10 text-lg leading-relaxed text-slate-600">
              {{ product()!.description || '目前沒有商品描述。' }}
            </p>

            <div class="mb-8 rounded-3xl border border-slate-100 bg-slate-50 p-6">
              <span class="mb-4 block text-xs font-bold uppercase tracking-[0.2em] text-slate-400">
                選擇顏色: <span class="ml-2 text-slate-900">{{ selectedVariant()?.color ?? '未提供' }}</span>
              </span>
              <div class="flex flex-wrap gap-4">
                @for (variant of product()!.variants; track variant.id) {
                  <button
                    type="button"
                    (click)="onVariantChange(variant)"
                    [class.ring-2]="selectedVariant()?.id === variant.id"
                    [class.ring-offset-2]="selectedVariant()?.id === variant.id"
                    [class.ring-indigo-600]="selectedVariant()?.id === variant.id"
                    class="group relative flex h-12 w-12 items-center justify-center rounded-full border border-white shadow-md transition-all hover:scale-110"
                    [style.backgroundColor]="getColorHex(variant.color)"
                    [attr.aria-label]="variant.color"
                    [title]="variant.color"
                  >
                    <span class="absolute -top-10 scale-0 whitespace-nowrap rounded-md bg-slate-900 px-2 py-1 text-[10px] text-white transition-all group-hover:scale-100">
                      {{ variant.color }}
                    </span>
                  </button>
                }
              </div>
            </div>

            <div class="mb-10">
              <span class="mb-4 block text-xs font-bold uppercase tracking-[0.2em] text-slate-400">選擇尺寸</span>
              <div class="flex flex-wrap gap-3">
                @for (sku of selectedVariant()?.skus ?? []; track sku.id) {
                  <button
                    type="button"
                    (click)="selectedSKU.set(sku)"
                    [disabled]="sku.stock === 0"
                    [class]="
                      selectedSKU()?.id === sku.id
                        ? 'bg-slate-900 text-white border-slate-900 shadow-xl scale-105'
                        : 'bg-white text-slate-900 border-slate-200 hover:border-indigo-600 hover:text-indigo-600'
                    "
                    class="flex h-14 min-w-[72px] items-center justify-center rounded-2xl border-2 text-sm font-black transition-all disabled:cursor-not-allowed disabled:opacity-30"
                  >
                    {{ sizeService.getSizeName(sku.sizeId) }}
                  </button>
                }
              </div>
            </div>

            <div class="mt-auto flex items-center gap-4">
              <div class="flex-1">
                <button
                  pButton
                  label="加入購物車"
                  icon="pi pi-shopping-cart"
                  class="w-full rounded-2xl border-none bg-indigo-600 py-5 font-bold text-white shadow-2xl transition-all active:scale-95 hover:bg-indigo-700"
                  [disabled]="!activeSKU() || (activeSKU()?.stock ?? 0) <= 0 || cartService.isMutating()"
                  (click)="addToCart()"
                ></button>
              </div>
            </div>

            <div class="mt-10 flex gap-8 border-t border-slate-100 pt-8">
              <div class="flex items-center gap-2 text-xs font-bold uppercase tracking-widest text-slate-400">
                <i class="pi pi-truck text-indigo-600"></i>
                <span>快速配送</span>
              </div>
              <div class="flex items-center gap-2 text-xs font-bold uppercase tracking-widest text-slate-400">
                <i class="pi pi-shield text-indigo-600"></i>
                <span>安全支付</span>
              </div>
            </div>
          </div>
        </div>
      }
    </div>
  `,
  styles: [
    `
      @reference "tailwindcss";
    `,
  ],
})
export class ProductDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly categoryService = inject(CategoryService);
  private readonly productService = inject(ProductService);
  readonly sizeService = inject(SizeService);
  private readonly tagService = inject(TagService);
  protected readonly cartService = inject(CartService);

  private readonly colorMap: Record<string, string> = {
    black: '#1A1A1A',
    white: '#F8FAFC',
    navy: '#1E3A8A',
    blue: '#2563EB',
    red: '#DC2626',
    green: '#16A34A',
    gray: '#6B7280',
    grey: '#6B7280',
    beige: '#D6D3D1',
    brown: '#92400E',
    pink: '#EC4899',
    purple: '#7C3AED',
    yellow: '#EAB308',
    orange: '#F97316',
  };

  protected readonly isLoading = signal(true);
  protected readonly product = signal<Product | null>(null);
  protected readonly selectedVariant = signal<ProductVariant | null>(null);
  protected readonly selectedSKU = signal<SKU | null>(null);
  protected readonly activeImageIndex = signal(0);
  protected readonly activeVariantImages = signal<ProductImage[]>([]);
  protected readonly activeImage = computed(() => this.activeVariantImages()[this.activeImageIndex()] ?? null);

  protected readonly categoryName = computed(() =>
    this.product() ? this.categoryService.getCategoryName(this.product()!.categoryId) : '商品'
  );

  protected readonly audienceQueryParams = computed(() => {
    const audience = this.product()?.audience;
    return audience ? { audience } : {};
  });

  protected readonly categoryQueryParams = computed(() => {
    const categoryId = this.product()?.categoryId;
    return categoryId ? { category: categoryId } : {};
  });

  protected readonly audienceName = computed(() => {
    const audience = this.product()?.audience;
    if (audience === 'Men') return '男裝';
    if (audience === 'Women') return '女裝';
    return '商品';
  });

  protected readonly displayTags = computed(() =>
    (this.product()?.tagIds ?? []).map((id) => this.tagService.getTagName(id))
  );

  protected readonly activeSKU = computed(() => this.selectedSKU() ?? this.selectedVariant()?.skus[0] ?? null);

  protected readonly minPrice = computed(() => {
    const prices = (this.product()?.variants ?? []).flatMap((variant) => variant.skus.map((sku) => sku.price));
    return prices.length > 0 ? Math.min(...prices) : 0;
  });

  protected readonly totalStock = computed(() =>
    (this.product()?.variants ?? []).flatMap((variant) => variant.skus).reduce((sum, sku) => sum + sku.stock, 0)
  );

  protected readonly stockLabel = computed(() => {
    const sku = this.activeSKU();
    if (sku) {
      return sku.stock > 0 ? `庫存: ${sku.stock}` : '缺貨中';
    }

    return this.totalStock() > 0 ? `總庫存: ${this.totalStock()}` : '缺貨中';
  });

  protected readonly stockBadgeClass = computed(() =>
    (this.activeSKU()?.stock ?? this.totalStock()) > 0
      ? 'text-emerald-600 bg-emerald-50'
      : 'text-rose-600 bg-rose-50'
  );

  ngOnInit(): void {
    this.route.paramMap.subscribe((params) => {
      const productId = params.get('id');

      if (!productId) {
        this.product.set(null);
        this.isLoading.set(false);
        return;
      }

      this.loadProduct(productId);
    });
  }

  protected onVariantChange(variant: ProductVariant): void {
    this.selectedVariant.set(variant);
    this.activeVariantImages.set([...variant.images].sort((a, b) => a.sortOrder - b.sortOrder));
    this.selectedSKU.set(variant.skus.find((sku) => sku.stock > 0) ?? variant.skus[0] ?? null);
    this.activeImageIndex.set(0);
  }

  protected getColorHex(colorName: string): string {
    const normalized = colorName.trim().toLowerCase();
    return this.colorMap[normalized] ?? colorName;
  }

  protected addToCart(): void {
    const sku = this.activeSKU();
    if (!sku || sku.stock <= 0) {
      return;
    }

    this.cartService.addItem(sku.id, 1).subscribe();
  }

  private loadProduct(productId: string): void {
    this.isLoading.set(true);
    this.product.set(null);
    this.selectedVariant.set(null);
    this.selectedSKU.set(null);
    this.activeVariantImages.set([]);
    this.activeImageIndex.set(0);

    this.productService.getById(productId).subscribe({
      next: (product) => {
        this.product.set(product);
        if (product.variants.length > 0) {
          this.onVariantChange(product.variants[0]);
        }
        this.isLoading.set(false);
      },
      error: () => {
        this.product.set(null);
        this.isLoading.set(false);
      },
    });
  }
}
