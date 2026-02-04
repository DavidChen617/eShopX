import { Component, signal } from '@angular/core';

import { ActivatedRoute, Router } from '@angular/router';

import { ProductService } from '../../core/services/product.service';
import { GetProductItems } from '../../core/models/api-models';

@Component({
  selector: 'app-search-page',
  standalone: true,
  imports: [],
  templateUrl: './search.page.html',
})
export class SearchPageComponent {
  query = signal('');
  products = signal<GetProductItems[]>([]);
  isLoading = signal(true);

  constructor(
    private readonly productService: ProductService,
    private readonly route: ActivatedRoute,
    private readonly router: Router
  ) {
    this.route.queryParamMap.subscribe((params) => {
      const q = params.get('q') ?? '';
      this.query.set(q);
      void this.load(q);
    });
  }

  async submit(): Promise<void> {
    const q = this.query().trim();
    await this.router.navigate(['/search'], { queryParams: q ? { q } : {} });
  }

  private async load(query: string): Promise<void> {
    this.isLoading.set(true);
    const response = query
      ? await this.productService.searchProducts(query)
      : await this.productService.getProducts();
    this.products.set(response.items);
    this.isLoading.set(false);
  }
}
