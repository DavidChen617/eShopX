import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { NavbarComponent } from './navbar.component';
import { CartDrawerComponent } from './cart-drawer.component';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, NavbarComponent, CartDrawerComponent],
  template: `
    <div class="min-h-screen bg-slate-50">
      <app-navbar></app-navbar>
      <app-cart-drawer></app-cart-drawer>
      <main>
        <router-outlet></router-outlet>
      </main>
...

      <footer class="bg-white border-t border-slate-200 py-12 mt-20">
        <div class="max-w-7xl mx-auto px-4 text-center text-slate-500">
          &copy; 2026 eShopX. All rights reserved.
        </div>
      </footer>
    </div>
  `,
})
export class MainLayoutComponent {}
