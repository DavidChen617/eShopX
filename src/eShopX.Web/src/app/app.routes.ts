import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./layout/main-layout.component').then(m => m.MainLayoutComponent),
    children: [
      {
        path: '',
        loadComponent: () => import('./features/home/home.component').then(m => m.HomeComponent)
      },
      {
        path: 'product/:id',
        loadComponent: () => import('./features/products/product-detail.component').then(m => m.ProductDetailComponent)
      },
      {
        path: 'products',
        loadComponent: () => import('./features/products/product-list.component').then(m => m.ProductListComponent)
      },
      {
        path: 'checkout',
        loadComponent: () => import('./features/checkout/checkout.component').then(m => m.CheckoutComponent)
      },
      {
        path: 'orders',
        loadComponent: () => import('./features/orders/orders.component').then(m => m.OrdersComponent)
      },
      {
        path: 'orders/:orderId',
        loadComponent: () => import('./features/orders/order-detail.component').then(m => m.OrderDetailComponent)
      }
    ]
  },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register.component').then(m => m.RegisterComponent)
  },
  {
    path: 'auth/google/callback',
    loadComponent: () => import('./features/auth/google-callback.component').then(m => m.GoogleCallbackComponent)
  },
  {
    path: 'auth/line/callback',
    loadComponent: () => import('./features/auth/line-callback.component').then(m => m.LineCallbackComponent)
  },
  {
    path: 'admin',
    loadComponent: () => import('./layout/admin-layout.component').then(m => m.AdminLayoutComponent),
    children: [
      {
        path: 'products',
        loadComponent: () => import('./features/admin/product-management.component').then(m => m.ProductManagementComponent)
      },
      {
        path: 'products/new',
        loadComponent: () => import('./features/admin/product-form.component').then(m => m.ProductFormComponent)
      },
      {
        path: 'products/:id/edit',
        loadComponent: () => import('./features/admin/product-form.component').then(m => m.ProductFormComponent)
      },
      {
        path: 'orders',
        loadComponent: () => import('./features/admin/order-management.component').then(m => m.OrderManagementComponent)
      },
      {
        path: 'categories',
        loadComponent: () => import('./features/admin/category-size-management.component').then(m => m.CategorySizeManagementComponent)
      },
      {
        path: '',
        redirectTo: 'products',
        pathMatch: 'full'
      }
    ]
  }
];
