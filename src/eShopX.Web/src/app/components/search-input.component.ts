import { CommonModule } from '@angular/common';
import { Component, computed, effect, inject, input, output, signal } from '@angular/core';
import { Router } from '@angular/router';
import { ProductSummary } from '../models/api.models';
import { ProductService } from '../services/product.service';

@Component({
  selector: 'app-search-input',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="relative w-full">
      <label class="relative block w-full">
        <i
          class="pi pi-search pointer-events-none absolute left-4 top-1/2 -translate-y-1/2 text-sm text-slate-400"
        ></i>
        <input
          #searchInput
          type="text"
          [value]="keyword()"
          [placeholder]="placeholder()"
          [attr.aria-expanded]="shouldShowDropdown()"
          [attr.aria-controls]="dropdownId"
          aria-autocomplete="list"
          aria-label="搜尋商品"
          autocomplete="off"
          (input)="onInput($event)"
          (compositionstart)="onCompositionStart()"
          (compositionend)="onCompositionEnd($event)"
          (focus)="onFocus()"
          (blur)="onBlur()"
          (keydown.escape)="closeDropdown()"
          (keydown.enter)="onEnter($event)"
          class="w-full rounded-2xl border border-transparent bg-slate-100 py-2.5 pl-12 pr-4 text-sm text-slate-900 transition-all placeholder:text-slate-400 hover:bg-slate-200 focus:border-indigo-200 focus:bg-white focus:outline-none focus:ring-4 focus:ring-indigo-100 md:text-base"
        />
      </label>

      @if (shouldShowDropdown()) {
        <div
          [id]="dropdownId"
          role="listbox"
          class="absolute left-0 right-0 top-[calc(100%+0.5rem)] z-50 overflow-hidden rounded-3xl border border-slate-200 bg-white shadow-2xl shadow-slate-900/10"
        >
          @if (isLoading()) {
            <div class="px-4 py-5 text-sm text-slate-500">搜尋中...</div>
          } @else if (results().length > 0) {
            <div class="max-h-96 overflow-y-auto py-2">
              @for (product of results(); track product.productId) {
                <button
                  type="button"
                  (mousedown)="openProduct(product.productId, $event)"
                  class="flex w-full items-center gap-3 px-4 py-3 text-left transition-colors hover:bg-slate-50 focus:bg-slate-50 focus:outline-none"
                >
                  <img
                    [src]="product.primaryImageUrl"
                    [alt]="product.name"
                    class="h-14 w-14 shrink-0 rounded-2xl bg-slate-100 object-cover"
                  />
                  <div class="min-w-0 flex-1">
                    <p class="truncate text-sm font-bold text-slate-900">{{ product.name }}</p>
                    <p class="mt-1 line-clamp-1 text-xs text-slate-500">{{ product.description }}</p>
                  </div>
                  <div class="shrink-0 text-right">
                    <p class="text-xs text-slate-400">NT$</p>
                    <p class="text-sm font-black text-slate-900">
                      {{ product.price | number : '1.0-0' }}
                    </p>
                  </div>
                </button>
              }
            </div>

            <button
              type="button"
              (mousedown)="onSearch()"
              class="flex w-full items-center justify-center gap-2 border-t border-slate-100 px-4 py-3 text-sm font-bold text-indigo-600 transition-colors hover:bg-indigo-50"
            >
              <span>查看「{{ keyword().trim() }}」全部結果</span>
              <i class="pi pi-arrow-right text-xs"></i>
            </button>
          } @else {
            <div class="px-4 py-5 text-sm text-slate-500">找不到符合的商品</div>
          }
        </div>
      }
    </div>
  `,
})
export class SearchInputComponent {
  private readonly router = inject(Router);
  private readonly productService = inject(ProductService);

  readonly placeholder = input('搜尋...');
  readonly value = input('');
  readonly showSearchEntries = input(false);
  readonly autoNavigateOnEnter = input(false);

  readonly valueChange = output<string>();
  readonly search = output<string>();

  protected readonly dropdownId = `search-input-dropdown-${Math.random().toString(36).slice(2, 9)}`;

  protected readonly keyword = signal(this.value());
  protected readonly isFocused = signal(false);
  protected readonly isLoading = signal(false);
  protected readonly isComposing = signal(false);
  protected readonly results = signal<ProductSummary[]>([]);
  protected readonly shouldShowDropdown = computed(() => {
    const keyword = this.keyword().trim();
    return this.showSearchEntries()
      && this.isFocused()
      && !this.isComposing()
      && keyword.length >= 2;
  });

  private searchTimer: ReturnType<typeof setTimeout> | null = null;

  constructor() {
    effect(() => {
      this.keyword.set(this.value());
    });
  }

  protected onInput(event: Event): void {
    const nativeEvent = event as InputEvent;
    const nextValue = (event.target as HTMLInputElement).value;
    const isComposing = nativeEvent.isComposing;

    this.isComposing.set(isComposing);
    this.keyword.set(nextValue);

    if (isComposing) {
      this.isLoading.set(false);
      this.results.set([]);
      return;
    }

    this.valueChange.emit(nextValue);

    if (!this.showSearchEntries()) {
      return;
    }

    this.queueSearch(nextValue);
  }

  protected onCompositionStart(): void {
    this.isComposing.set(true);
    this.isLoading.set(false);
    this.results.set([]);
  }

  protected onCompositionEnd(event: Event): void {
    const nextValue = (event.target as HTMLInputElement).value;
    this.isComposing.set(false);
    this.keyword.set(nextValue);
    this.valueChange.emit(nextValue);

    if (!this.showSearchEntries()) {
      return;
    }

    this.queueSearch(nextValue);
  }

  protected onFocus(): void {
    this.isFocused.set(true);

    if (!this.showSearchEntries()) {
      return;
    }

    this.queueSearch(this.keyword());
  }

  protected onBlur(): void {
    window.setTimeout(() => this.closeDropdown(), 120);
  }

  protected onEnter(event: Event): void {
    if ((event as KeyboardEvent).isComposing) {
      return;
    }

    this.onSearch();
  }

  protected onSearch(): void {
    const keyword = this.keyword().trim();
    this.closeDropdown();
    this.search.emit(keyword);

    if (!keyword || !this.autoNavigateOnEnter()) {
      return;
    }

    void this.router.navigate(['/products'], {
      queryParams: { keyword },
    });
  }

  protected closeDropdown(): void {
    this.isFocused.set(false);
    this.isComposing.set(false);
  }

  protected openProduct(productId: string, event: MouseEvent): void {
    event.preventDefault();
    this.closeDropdown();
    void this.router.navigateByUrl(`/product/${productId}`);
  }

  private queueSearch(rawKeyword: string): void {
    if (!this.showSearchEntries()) {
      this.isLoading.set(false);
      this.results.set([]);
      return;
    }

    const keyword = rawKeyword.trim();

    if (this.searchTimer) {
      clearTimeout(this.searchTimer);
    }

    if (keyword.length < 2) {
      this.isLoading.set(false);
      this.results.set([]);
      return;
    }

    this.searchTimer = setTimeout(() => {
      this.isLoading.set(true);
      this.productService.searchEntries(keyword).subscribe({
        next: (items) => {
          if (this.keyword().trim() !== keyword) {
            return;
          }

          this.results.set(items);
          this.isLoading.set(false);
        },
        error: () => {
          if (this.keyword().trim() !== keyword) {
            return;
          }

          this.results.set([]);
          this.isLoading.set(false);
        },
      });
    }, 220);
  }
}
