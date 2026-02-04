import { Routes } from '@angular/router';

import { OrdersPageComponent } from './orders.page';
import { OrderDetailPageComponent } from './order-detail.page';

export const ORDERS_ROUTES: Routes = [
  {
    path: '',
    component: OrdersPageComponent,
  },
  {
    path: ':id',
    component: OrderDetailPageComponent,
  },
];
