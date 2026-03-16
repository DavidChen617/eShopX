import { CommonModule } from '@angular/common';
import { Component, computed, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { TablePageEvent } from 'primeng/table';
import { TableModule } from 'primeng/table';
import { ToastModule } from 'primeng/toast';
import { ToggleSwitchModule } from 'primeng/toggleswitch';
import { AdminPrimaryButtonComponent } from '../../components/admin-primary-button.component';
import { SearchInputComponent } from '../../components/search-input.component';
import { ProductListItemResponse } from '../../models/api.models';
import { AdminProductService } from '../../services/admin-product.service';
import { CategoryService } from '../../services/category.service';

interface AdminProductItem {
  id: string;
  name: string;
  image: string;
  categoryId: string;
  colors: string[];
  audience: string | null;
  isActive: boolean;
  startingPrice: number;
}

@Component({
  selector: 'app-product-management',
  standalone: true,
  imports: [
    CommonModule,
    TableModule,
    ButtonModule,
    ToggleSwitchModule,
    ToastModule,
    FormsModule,
    RouterLink,
    AdminPrimaryButtonComponent,
    SearchInputComponent,
  ],
  providers: [MessageService],
  template: `
    <div class="overflow-hidden rounded-3xl border border-slate-200 bg-white shadow-sm">
      <p-toast position="top-right"></p-toast>

      <div class="flex flex-col justify-between gap-4 border-b border-slate-100 p-6 md:flex-row md:items-center">
        <div>
          <h1 class="text-2xl font-black text-slate-900">商品管理</h1>
          <p class="text-sm text-slate-500">管理您的商店庫存、價格與發佈狀態</p>
        </div>
        <div class="flex items-center gap-3">
          <div class="w-full md:w-64">
            <app-search-input
              [value]="keyword"
              placeholder="搜尋商品"
              (valueChange)="onKeywordChange($event)"
            ></app-search-input>
          </div>
          <app-admin-primary-button
            label="新增商品"
            icon="pi pi-plus"
            [routerLink]="'/admin/products/new'"
          />
        </div>
      </div>

      <p-table
        [value]="products()"
        [paginator]="true"
        [rows]="rows()"
        [first]="first()"
        [totalRecords]="totalRecords()"
        [rowsPerPageOptions]="[10, 20, 50]"
        [lazy]="true"
        (onPage)="onPageChange($event)"
        [responsiveLayout]="'scroll'"
        currentPageReportTemplate="{first} - {last} / 共 {totalRecords} 件商品"
        [showCurrentPageReport]="true"
        styleClass="p-datatable-gridlines"
      >
        <ng-template pTemplate="header">
          <tr class="bg-slate-50/50 text-xs uppercase tracking-widest text-slate-400">
            <th class="px-6 py-4">商品資訊</th>
            <th class="px-6 py-4 text-center">分類</th>
            <th class="px-6 py-4 text-center">顏色</th>
            <th class="px-6 py-4 text-center">受眾</th>
            <th class="px-6 py-4 text-center">起始價格</th>
            <th class="px-6 py-4 text-center">上/下架</th>
            <th class="px-6 py-4 text-right">操作</th>
          </tr>
        </ng-template>
        <ng-template pTemplate="body" let-product>
          <tr class="border-b border-slate-100 transition-colors last:border-none hover:bg-slate-50/80">
            <td class="px-6 py-4">
              <div class="flex items-center gap-4">
                <div class="h-12 w-12 shrink-0 overflow-hidden rounded-xl border border-slate-200 bg-slate-100 shadow-sm">
                  <img [src]="product.image" class="h-full w-full object-cover" />
                </div>
                <div>
                  <div class="font-black text-slate-900">{{ product.name }}</div>
                  <div class="font-mono text-[10px] text-slate-400">{{ product.id }}</div>
                </div>
              </div>
            </td>
            <td class="px-6 py-4 text-center">
              <span class="text-sm font-bold text-slate-700">
                {{ categoryService.getCategoryName(product.categoryId) }}
              </span>
            </td>
            <td class="px-6 py-4 text-center">
              <span class="text-sm text-slate-600">
                {{ product.colors.join('、') || '未設定' }}
              </span>
            </td>
            <td class="px-6 py-4 text-center">
              <span class="text-sm font-bold text-slate-700">
                {{ audienceLabel(product.audience) }}
              </span>
            </td>
            <td class="px-6 py-4 text-center">
              <span class="font-mono font-bold text-slate-700">TWD {{ product.startingPrice }}</span>
            </td>
            <td class="px-6 py-4 text-center">
              <p-toggleSwitch
                [ngModel]="product.isActive"
                (onChange)="toggleProductStatus(product, $event.checked)"
              ></p-toggleSwitch>
            </td>
            <td class="px-6 py-4 text-right">
              <div class="flex justify-end gap-2">
                <button
                  pButton
                  icon="pi pi-pencil"
                  [routerLink]="['/admin/products', product.id, 'edit']"
                  class="p-button-text p-button-rounded text-slate-400 hover:text-indigo-600"
                ></button>
                <button
                  pButton
                  icon="pi pi-trash"
                  (click)="removeProduct(product.id)"
                  class="p-button-text p-button-rounded text-slate-400 hover:text-rose-500"
                ></button>
              </div>
            </td>
          </tr>
        </ng-template>
      </p-table>
    </div>
  `,
  styles: [`
    @reference "tailwindcss";
    :host ::ng-deep .p-datatable {
      @apply border-none;
    }
    :host ::ng-deep .p-datatable .p-datatable-thead > tr > th {
      @apply border-none bg-slate-50/50 font-black text-[10px] text-slate-500;
    }
    :host ::ng-deep .p-datatable .p-datatable-tbody > tr > td {
      @apply border-none py-4;
    }
    :host ::ng-deep .p-toast {
      @apply opacity-100;
    }
    :host ::ng-deep .p-toast .p-toast-message {
      @apply rounded-2xl border border-slate-200 bg-white shadow-xl;
    }
    :host ::ng-deep .p-toast .p-toast-message-content {
      @apply items-start gap-3 px-4 py-4;
    }
    :host ::ng-deep .p-toast .p-toast-summary {
      @apply text-sm font-bold text-slate-900;
    }
    :host ::ng-deep .p-toast .p-toast-detail {
      @apply mt-1 text-sm text-slate-600;
    }
    :host ::ng-deep .p-toggleswitch.p-toggleswitch-checked .p-toggleswitch-slider {
      @apply bg-indigo-600;
    }
    :host ::ng-deep .p-toggleswitch {
      @apply inline-flex items-center;
    }
    :host ::ng-deep .p-toggleswitch .p-toggleswitch-slider {
      @apply relative h-6 w-11 rounded-full;
      transition: background-color 180ms ease, border-color 180ms ease;
    }
    :host ::ng-deep .p-toggleswitch .p-toggleswitch-handle {
      @apply absolute left-0.5 h-5 w-5 rounded-full bg-white shadow-sm;
      top: 50%;
      transform: translateY(-50%);
      transition: left 180ms ease, right 180ms ease, transform 180ms ease;
    }
    :host ::ng-deep .p-toggleswitch.p-toggleswitch-checked .p-toggleswitch-handle {
      left: auto;
      right: 0.125rem;
      transform: translateY(-50%);
    }
    :host ::ng-deep .p-paginator {
      @apply border-none border-t border-slate-100 bg-white px-4 py-3;
    }
    :host ::ng-deep .p-paginator .p-paginator-page,
    :host ::ng-deep .p-paginator .p-paginator-prev,
    :host ::ng-deep .p-paginator .p-paginator-next,
    :host ::ng-deep .p-paginator .p-paginator-first,
    :host ::ng-deep .p-paginator .p-paginator-last {
      @apply h-10 min-w-10 rounded-xl text-slate-500 transition-colors;
    }
    :host ::ng-deep .p-paginator .p-paginator-page.p-paginator-page-selected {
      @apply bg-indigo-600 font-bold text-white;
    }
  `],
})
export class ProductManagementComponent implements OnInit {
  categoryService = inject(CategoryService);
  private adminProductService = inject(AdminProductService);
  private messageService = inject(MessageService);
  keyword = '';
  private searchTimer: ReturnType<typeof setTimeout> | null = null;

  totalRecords = computed(() => this.adminProductService.totalCount());
  rows = computed(() => this.adminProductService.pageSize());
  first = computed(() => (this.adminProductService.currentPage() - 1) * this.adminProductService.pageSize());

  products = computed<AdminProductItem[]>(() =>
    this.adminProductService.products().map((product: ProductListItemResponse) => ({
      id: product.id,
      name: product.name,
      image: product.primaryImageUrl || 'https://placehold.co/100x100/e2e8f0/64748b?text=No+Image',
      categoryId: product.categoryId,
      colors: product.colors,
      audience: product.audience,
      isActive: product.isActive,
      startingPrice: product.startingPrice,
    }))
  );

  ngOnInit() {
    this.adminProductService.loadProducts().subscribe();
  }

  onPageChange(event: TablePageEvent) {
    const pageSize = event.rows || this.rows();
    const page = Math.floor((event.first || 0) / pageSize) + 1;
    const keyword = this.keyword.trim();
    const request$ = keyword
      ? this.adminProductService.searchProducts(keyword, page, pageSize)
      : this.adminProductService.loadProducts(page, pageSize);
    request$.subscribe();
  }

  onKeywordChange(value: string) {
    this.keyword = value;

    if (this.searchTimer) {
      clearTimeout(this.searchTimer);
    }

    this.searchTimer = setTimeout(() => {
      const keyword = this.keyword.trim();
      const pageSize = this.rows();
      const request$ = keyword
        ? this.adminProductService.searchProducts(keyword, 1, pageSize)
        : this.adminProductService.loadProducts(1, pageSize);
      request$.subscribe();
    }, 220);
  }

  audienceLabel(audience: string | null): string {
    if (audience === 'Men') return '男裝';
    if (audience === 'Women') return '女裝';
    return '-';
  }

  toggleProductStatus(product: AdminProductItem, checked: boolean) {
    const request$ = checked
      ? this.adminProductService.publish(product.id)
      : this.adminProductService.unpublish(product.id);

    request$.subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: '商品狀態已更新',
          detail: checked ? '商品已上架' : '商品已下架',
          life: 2200,
        });
      },
      error: () => {
        this.messageService.add({
          severity: 'error',
          summary: '商品狀態更新失敗',
          detail: '請稍後再試，或確認後端 API 是否可用。',
          life: 2600,
        });
      },
    });
  }

  removeProduct(productId: string) {
    this.adminProductService.delete(productId).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: '商品已刪除',
          detail: productId,
          life: 2200,
        });
      },
      error: () => {
        this.messageService.add({
          severity: 'error',
          summary: '商品刪除失敗',
          detail: '請稍後再試，或確認後端 API 是否可用。',
          life: 2600,
        });
      },
    });
  }
}
