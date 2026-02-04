import { Component, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { ProductService } from '../../core/services/product.service';
import { HomepageService } from '../../core/services/homepage.service';
import { GetProductItems } from '../../core/models/api-models';
import { SectionComponent } from '../../shared/layout/section/section.component';
import { ProductCardComponent } from '../../shared/ui/product-card/product-card.component';

@Component({
  selector: 'app-products-page',
  standalone: true,
  imports: [SectionComponent, ProductCardComponent],
  templateUrl: './products.page.html',
})
export class ProductsPageComponent {
  products = signal<GetProductItems[]>([]);
  isLoading = signal(true);
  title = signal('Products');
  subtitle = signal('Browse the latest essentials.');
  private categories = signal<{ id: string; name: string }[]>([]);
  page = signal(1);
  pageSize = signal(12);
  totalPages = signal(0);
  private currentQuery = '';
  private currentCategoryId = '';

  constructor(
    private readonly productService: ProductService,
    private readonly homepageService: HomepageService,
    private readonly route: ActivatedRoute
  ) {
    this.route.queryParamMap.subscribe((params) => {
      const q = params.get('q') ?? '';
      const categoryId = params.get('categoryId') ?? '';
      const categoryName = params.get('categoryName') ?? '';
      this.page.set(1);
      void this.load(q, categoryId, categoryName);
    });
  }

  private async load(query = '', categoryId = '', categoryName = ''): Promise<void> {
    this.isLoading.set(true);
    this.currentQuery = query;
    this.currentCategoryId = categoryId;
    await this.ensureCategories();
    const response = await this.productService.getProducts({
      keyword: query.trim() || undefined,
      categoryId: categoryId.trim() || undefined,
      page: this.page(),
      pageSize: this.pageSize(),
    });
    this.products.set(response.items);
    this.totalPages.set(response.totalPages ?? 0);
    this.updateTitle(query, categoryId, categoryName);
    this.isLoading.set(false);
  }

  private async ensureCategories(): Promise<void> {
    if (this.categories().length > 0) return;
    const list = await this.homepageService.getCategories();
    this.categories.set(list.map(cat => ({ id: cat.id, name: cat.name })));
  }

  private updateTitle(query: string, categoryId: string, categoryName: string): void {
    if (categoryId) {
      if (categoryName) {
        this.title.set(categoryName);
        this.subtitle.set('分類商品');
        return;
      }
      const match = this.categories().find(cat => cat.id === categoryId);
      if (match) {
        this.title.set(match.name);
        this.subtitle.set('分類商品');
        return;
      }
      this.title.set('分類商品');
      this.subtitle.set('分類商品');
      return;
    }
    if (query.trim()) {
      this.title.set(`搜尋：${query}`);
      this.subtitle.set('搜尋結果');
      return;
    }
    this.title.set('Products');
    this.subtitle.set('Browse the latest essentials.');
  }

  async goToPage(nextPage: number): Promise<void> {
    if (this.isLoading()) return;
    if (nextPage < 1 || (this.totalPages() > 0 && nextPage > this.totalPages())) return;
    this.page.set(nextPage);
    await this.load(this.currentQuery, this.currentCategoryId);
  }
}
