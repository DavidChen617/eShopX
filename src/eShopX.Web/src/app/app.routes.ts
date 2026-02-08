import { Routes } from '@angular/router';

import { AppShellComponent } from './layout/app-shell/app-shell.component';

export const routes: Routes = [
  {
    path: '',
    component: AppShellComponent,
    children: [
      {
        path: '',
        loadChildren: () => import('./features/home/home.routes').then((m) => m.HOME_ROUTES),
      },
      {
        path: 'products',
        loadChildren: () =>
          import('./features/products/products.routes').then((m) => m.PRODUCTS_ROUTES),
      },
      {
        path: 'cart',
        loadChildren: () => import('./features/cart/cart.routes').then((m) => m.CART_ROUTES),
      },
      {
        path: 'orders',
        loadChildren: () => import('./features/orders/orders.routes').then((m) => m.ORDERS_ROUTES),
      },
      {
        path: 'account',
        loadChildren: () =>
          import('./features/account/account.routes').then((m) => m.ACCOUNT_ROUTES),
      },
      {
        path: 'auth',
        loadChildren: () => import('./features/auth/auth.routes').then((m) => m.AUTH_ROUTES),
      },
      {
        path: 'pay/line/success',
        loadComponent: () =>
          import('./features/pay/line-pay-success.page').then((m) => m.LinePaySuccessPageComponent),
      },
      {
        path: 'pay/line/fail',
        loadComponent: () =>
          import('./features/pay/line-pay-fail.page').then((m) => m.LinePayFailPageComponent),
      },
      {
        path: 'pay/line/cancel',
        loadComponent: () =>
          import('./features/pay/line-pay-cancel.page').then((m) => m.LinePayCancelPageComponent),
      },
      {
        path: 'pay/paypal/success',
        loadComponent: () =>
          import('./features/pay/paypal-success.page').then((m) => m.PayPalSuccessPageComponent),
      },
      {
        path: 'pay/paypal/fail',
        loadComponent: () =>
          import('./features/pay/paypal-fail.page').then((m) => m.PayPalFailPageComponent),
      },
      {
        path: 'pay/paypal/cancel',
        loadComponent: () =>
          import('./features/pay/paypal-cancel.page').then((m) => m.PayPalCancelPageComponent),
      },
      {
        path: 'search',
        loadComponent: () =>
          import('./features/search/search.page').then((m) => m.SearchPageComponent),
      },
      {
        path: 'dashboard/products',
        loadComponent: () =>
          import('./features/dashboard/products/dashboard-products.page').then(
            (m) => m.DashboardProductsPageComponent,
          ),
      },
      {
        path: 'admin/sellers',
        loadComponent: () =>
          import('./features/admin/admin-sellers.page').then((m) => m.AdminSellersPageComponent),
      },
    ],
  },
  {
    path: '**',
    redirectTo: '',
  },
];
