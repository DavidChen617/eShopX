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
import { ProductSummary } from '../../models/api.models';
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
              <span class="text-sm text-slate-400 font-normal ml-2">({{ filteredProducts().length }} 個結果)</span>
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
            @if (filteredProducts().length === 0) {
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
                @for (prod of pagedProducts(); track prod.productId) {
                  <app-product-card [product]="mapToCardProduct(prod)"></app-product-card>
                }
              </div>

              <!-- Paginator -->
              <div class="mt-12">
                <p-paginator 
                  [first]="first()"
                  [rows]="rows()"
                  [totalRecords]="filteredProducts().length"
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
  selectedCategoryId = signal<string | null>(null);
  priceRange = signal<number[]>([0, 5000]);
  selectedSort = signal('newest');
  first = signal(0);
  rows = signal(6);

  sortOptions = [
    { label: '最新上架', value: 'newest' },
    { label: '價格：由低到高', value: 'price_asc' },
    { label: '價格：由高到低', value: 'price_desc' },
  ];

  filteredProducts = computed(() => {
    const filtered = this.products().filter((p) => {
      const matchCategory = !this.selectedCategoryId() || p.categoryId === this.selectedCategoryId();
      const matchPrice = p.price >= this.priceRange()[0] && p.price <= this.priceRange()[1];
      return matchCategory && matchPrice;
    });

    const sorted = [...filtered];

    if (this.selectedSort() === 'price_asc') {
      sorted.sort((a, b) => a.price - b.price);
    } else if (this.selectedSort() === 'price_desc') {
      sorted.sort((a, b) => b.price - a.price);
    }

    return sorted;
  });

  pagedProducts = computed(() => {
    const start = this.first();
    const end = start + this.rows();
    return this.filteredProducts().slice(start, end);
  });

  currentCategoryName = computed(() => {
    return this.selectedCategoryId() 
      ? this.categoryService.getCategoryName(this.selectedCategoryId()!) 
      : '所有商品';
  });

  constructor() {
    effect(() => {
      this.selectedCategoryId();
      this.priceRange();
      this.selectedSort();
      this.first.set(0);
    });
  }

  ngOnInit() {
    this.loadProducts();

    // 監聽路由參數（如果有 category 參數）
    this.route.queryParams.subscribe(params => {
      this.selectedCategoryId.set(params['category'] ?? null);
      this.first.set(0);
    });
  }

  resetFilters() {
    this.selectedCategoryId.set(null);
    this.priceRange.set([0, 5000]);
    this.selectedSort.set('newest');
    this.first.set(0);
  }

  onPageChange(event: PaginatorState) {
    this.first.set(event.first ?? 0);
    this.rows.set(event.rows ?? 6);
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

  private loadProducts() {
    this.isLoading.set(true);

    this.productService.search({
      isActive: true,
      page: 1,
      pageSize: 100,
    }).subscribe({
      next: (response) => {
        this.products.set(response.items);
        this.isLoading.set(false);
      },
      error: () => {
        this.products.set([]);
        this.isLoading.set(false);
      },
    });
  }
}
