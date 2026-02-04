import { Routes } from '@angular/router';

import { ProductsPageComponent } from './products.page';
import { ProductDetailPageComponent } from './product-detail.page';

export const PRODUCTS_ROUTES: Routes = [
  {
    path: '',
    component: ProductsPageComponent,
  },
  {
    path: ':id',
    component: ProductDetailPageComponent,
  },
];
