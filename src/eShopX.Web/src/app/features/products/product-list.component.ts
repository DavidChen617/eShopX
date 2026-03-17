import { Component, OnInit, signal, inject, computed, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { SliderModule } from 'primeng/slider';
import { CheckboxModule } from 'primeng/checkbox';
import { PaginatorModule } from 'primeng/paginator';
import { SkeletonModule } from 'primeng/skeleton';
import { SelectButtonModule } from 'primeng/selectbutton';
import { PaginatorState } from 'primeng/paginator';
import { ProductCardComponent } from '../../components/product-card.component';
import { CategoryService } from '../../services/category.service';
import { Audience, ProductSummary } from '../../models/api.models';
import { ProductService } from '../../services/product.service';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    FormsModule,
    SliderModule,
    CheckboxModule,
    PaginatorModule,
    SkeletonModule,
    SelectButtonModule,
    ProductCardComponent,
  ],
  template: `
    <div class="max-w-7xl mx-auto px-4 py-8">
      <!-- Breadcrumbs -->
      <nav class="flex items-center gap-2 text-sm text-slate-500 mb-8 bg-slate-100/50 self-start px-4 py-2 rounded-full w-fit">
        <a routerLink="/" class="hover:text-indigo-600 transition-colors flex items-center gap-1">
          <i class="pi pi-home text-[10px]"></i> 首頁
        </a>
        <i class="pi pi-chevron-right text-[8px] text-slate-300"></i>
        <span class="text-slate-900 font-bold">所有商品</span>
      </nav>

      <div class="flex flex-col lg:flex-row gap-8">
        <!-- Sidebar Filters -->
        <aside class="w-full lg:w-64 space-y-8 shrink-0">
          <!-- Categories -->
          <section>
            <h3 class="text-sm font-black text-slate-900 uppercase tracking-widest mb-4">商品分類</h3>
            <div class="flex flex-col gap-3">
              <div 
                (click)="selectedCategoryId.set(null)"
                class="flex items-center justify-between group cursor-pointer"
              >
                <span [class]="!selectedCategoryId() ? 'text-indigo-600 font-bold' : 'text-slate-600 group-hover:text-slate-900'">全部商品</span>
                @if (!selectedCategoryId()) { <div class="w-1.5 h-1.5 rounded-full bg-indigo-600"></div> }
              </div>
              @for (cat of categoryService.categories(); track cat.id) {
                <div 
                  (click)="selectedCategoryId.set(cat.id)"
                  class="flex items-center justify-between group cursor-pointer"
                >
                  <span [class]="selectedCategoryId() === cat.id ? 'text-indigo-600 font-bold' : 'text-slate-600 group-hover:text-slate-900'">
                    {{ cat.name }}
                  </span>
                  @if (selectedCategoryId() === cat.id) { <div class="w-1.5 h-1.5 rounded-full bg-indigo-600"></div> }
                </div>
              }
            </div>
          </section>

          <!-- Audience -->
          <section>
            <h3 class="text-sm font-black text-slate-900 uppercase tracking-widest mb-4">受眾</h3>
            <div class="flex flex-col gap-3">
              @for (option of audienceOptions; track option.value ?? 'all') {
                <div
                  (click)="selectedAudience.set(option.value)"
                  class="flex items-center justify-between group cursor-pointer"
                >
                  <span [class]="selectedAudience() === option.value ? 'text-indigo-600 font-bold' : 'text-slate-600 group-hover:text-slate-900'">
                    {{ option.label }}
                  </span>
                  @if (selectedAudience() === option.value) { <div class="w-1.5 h-1.5 rounded-full bg-indigo-600"></div> }
                </div>
              }
            </div>
          </section>

          <!-- Price Range -->
          <section>
            <div class="flex justify-between items-center mb-4">
              <h3 class="text-sm font-black text-slate-900 uppercase tracking-widest">價格區間</h3>
              <span class="text-xs font-bold text-indigo-600">TWD {{ priceRange()[0] }} - {{ priceRange()[1] }}</span>
            </div>
            <p-slider 
              [(ngModel)]="priceRange" 
              [range]="true" 
              [min]="0" 
              [max]="5000"
              styleClass="w-full"
            ></p-slider>
          </section>

          <!-- Sort Order (Mock) -->
          <section>
            <h3 class="text-sm font-black text-slate-900 uppercase tracking-widest mb-4">排序方式</h3>
            <p-selectButton 
              [options]="sortOptions" 
              [(ngModel)]="selectedSort" 
              optionLabel="label" 
              optionValue="value"
              styleClass="w-full flex flex-col gap-2 border-none"
            >
              <ng-template let-item pTemplate="item">
                <div class="w-full text-left px-2 py-1 text-xs font-bold">{{ item.label }}</div>
              </ng-template>
            </p-selectButton>
          </section>
        </aside>

        <!-- Main Product Grid -->
        <div class="flex-1">
          <!-- Results Header -->
          <div class="flex items-center justify-between mb-8">
            <h2 class="text-2xl font-black text-slate-900">
              {{ currentCategoryName() }}
            </h2>
          </div>

          <!-- Loading State (Skeleton) -->
          @if (isLoading()) {
            <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
              @for (i of [1,2,3,4,5,6]; track i) {
                <div class="bg-white rounded-2xl p-4 space-y-4">
                  <p-skeleton width="100%" height="250px" borderRadius="16px"></p-skeleton>
                  <p-skeleton width="40%" height="1rem"></p-skeleton>
                  <p-skeleton width="80%" height="1.5rem"></p-skeleton>
                  <p-skeleton width="30%" height="1.5rem"></p-skeleton>
                </div>
              }
            </div>
          } @else {
            <!-- Product Grid -->
            @if (displayProducts().length === 0) {
              <div class="flex flex-col items-center justify-center py-20 text-slate-400">
                <i class="pi pi-search text-6xl mb-4"></i>
                <p class="text-lg font-medium">找不到符合條件的商品</p>
                <button 
                  (click)="resetFilters()"
                  class="mt-4 text-indigo-600 font-bold hover:underline"
                >重設所有篩選</button>
              </div>
            } @else {
              <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
                @for (prod of displayProducts(); track prod.productId) {
                  <app-product-card [product]="mapToCardProduct(prod)"></app-product-card>
                }
              </div>

              <!-- Paginator -->
              <div class="mt-12">
                <p-paginator 
                  [first]="first()"
                  [rows]="rows()"
                  [totalRecords]="totalCount()"
                  [showCurrentPageReport]="true"
                  currentPageReportTemplate="{first} - {last} / 共 {totalRecords} 個商品"
                  (onPageChange)="onPageChange($event)"
                  styleClass="bg-transparent border-none"
                ></p-paginator>
              </div>
            }
          }
        </div>
      </div>
    </div>
  `,
  styles: [`
    @reference "tailwindcss";
    :host ::ng-deep .p-slider {
      @apply h-1.5 bg-slate-200 border-none;
    }
    :host ::ng-deep .p-slider-range {
      @apply bg-indigo-600;
    }
    :host ::ng-deep .p-slider-handle {
      @apply bg-white border-2 border-indigo-600 w-4 h-4 -mt-1.5 focus:shadow-none;
    }
    :host ::ng-deep .p-selectbutton .p-button {
      @apply border-none bg-slate-100 rounded-xl text-slate-600 mb-1 transition-all;
    }
    :host ::ng-deep .p-selectbutton .p-button.p-highlight {
      @apply bg-indigo-600 text-white shadow-md;
    }
  `]
})
export class ProductListComponent implements OnInit {
  categoryService = inject(CategoryService);
  private route = inject(ActivatedRoute);
  private productService = inject(ProductService);

  isLoading = signal(true);
  products = signal<ProductSummary[]>([]);
  totalCount = signal(0);
  initialized = signal(false);
  keyword = signal('');
  selectedCategoryId = signal<string | null>(null);
  selectedAudience = signal<Audience | null>(null);
  priceRange = signal<number[]>([0, 5000]);
  selectedSort = signal('newest');
  first = signal(0);
  rows = signal(10);

  audienceOptions: Array<{ label: string; value: Audience | null }> = [
    { label: '全部', value: null },
    { label: '男裝', value: 'Men' },
    { label: '女裝', value: 'Women' },
  ];

  sortOptions = [
    { label: '最新上架', value: 'newest' },
    { label: '價格：由低到高', value: 'price_asc' },
    { label: '價格：由高到低', value: 'price_desc' },
  ];

  displayProducts = computed(() => {
    const sorted = [...this.products()];

    if (this.selectedSort() === 'price_asc') {
      sorted.sort((a, b) => a.price - b.price);
    } else if (this.selectedSort() === 'price_desc') {
      sorted.sort((a, b) => b.price - a.price);
    }

    return sorted;
  });

  currentCategoryName = computed(() => {
    if (this.keyword()) {
      return `搜尋「${this.keyword()}」`;
    }

    return this.selectedCategoryId() 
      ? this.categoryService.getCategoryName(this.selectedCategoryId()!) 
      : '所有商品';
  });

  constructor() {
    effect(() => {
      if (!this.initialized()) {
        return;
      }

      this.selectedCategoryId();
      this.selectedAudience();
      this.priceRange();
      this.first.set(0);
    });

    effect(() => {
      if (!this.initialized()) {
        return;
      }

      const keyword = this.keyword();
      const categoryId = this.selectedCategoryId();
      const audience = this.selectedAudience();
      const [minPrice, maxPrice] = this.priceRange();
      const first = this.first();
      const rows = this.rows();

      this.loadProducts({
        keyword,
        categoryId,
        audience,
        minPrice,
        maxPrice,
        page: Math.floor(first / rows) + 1,
        pageSize: rows,
      });
    });
  }

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      this.keyword.set((params['keyword'] ?? '').trim());
      this.selectedCategoryId.set(params['category'] ?? null);
      this.selectedAudience.set(params['audience'] === 'Men' || params['audience'] === 'Women' ? params['audience'] : null);
      this.initialized.set(true);
    });
  }

  resetFilters() {
    this.selectedCategoryId.set(null);
    this.selectedAudience.set(null);
    this.priceRange.set([0, 5000]);
    this.selectedSort.set('newest');
    this.first.set(0);
  }

  onPageChange(event: PaginatorState) {
    this.first.set(event.first ?? 0);
    this.rows.set(event.rows ?? 10);
  }

  // 轉換格式以適應 ProductCardComponent 的 Input (因為目前 ProductCard 接口與 Summary 略有不同)
  mapToCardProduct(summary: ProductSummary) {
    return {
      id: summary.productId,
      name: summary.name,
      price: summary.price,
      image: summary.primaryImageUrl,
      category: this.categoryService.getCategoryName(summary.categoryId),
      createdAt: new Date().toISOString() // 模擬剛上架
    };
  }

  private loadProducts(params: {
    keyword: string;
    categoryId: string | null;
    audience: Audience | null;
    minPrice: number;
    maxPrice: number;
    page: number;
    pageSize: number;
  }) {
    this.isLoading.set(true);

    this.productService.search({
      keyword: params.keyword || undefined,
      categoryId: params.categoryId ?? undefined,
      audience: params.audience ?? undefined,
      minPrice: params.minPrice,
      maxPrice: params.maxPrice,
      isActive: true,
      page: params.page,
      pageSize: params.pageSize,
    }).subscribe({
      next: (response) => {
        this.products.set(response.items);
        this.totalCount.set(response.totalCount);
        this.isLoading.set(false);
      },
      error: () => {
        this.products.set([]);
        this.totalCount.set(0);
        this.isLoading.set(false);
      },
    });
  }
}
