import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { BadgeModule } from 'primeng/badge';
import { finalize } from 'rxjs';
import { SearchInputComponent } from '../components/search-input.component';
import { CartService } from '../services/cart.service';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink, ButtonModule, BadgeModule, SearchInputComponent],
  template: `
    <nav
      class="sticky top-0 z-50 bg-white/80 backdrop-blur-md border-b border-slate-200 px-4 py-3"
    >
      <div class="max-w-7xl mx-auto flex items-center justify-between gap-2 md:gap-4">
        <!-- Logo -->
        <div routerLink="/" class="flex items-center gap-2 cursor-pointer group shrink-0">
          <div
            class="w-10 h-10 bg-indigo-600 rounded-xl flex items-center justify-center text-white shadow-lg group-hover:bg-indigo-700 transition-colors"
          >
            <i class="pi pi-shopping-bag text-xl"></i>
          </div>
          <span class="hidden sm:inline text-xl font-bold tracking-tight text-slate-900"
            >eShopX</span
          >
        </div>

        <!-- Search Bar (Visible on all screens, flexible width) -->
        <div class="flex-1 max-w-md mx-2">
          <app-search-input
            [showSearchEntries]="true"
            [autoNavigateOnEnter]="true"
          ></app-search-input>
        </div>

        <!-- Action Buttons -->
        <div class="flex items-center gap-1 md:gap-2 shrink-0">
          @if (authService.isAuthenticated()) {
            <button
              pButton
              icon="pi pi-sign-out"
              [loading]="isLoggingOut()"
              (click)="logout()"
              class="p-button-rounded p-button-text text-slate-600 hover:text-rose-600 p-1 md:hidden"
            ></button>
            <div class="hidden md:flex items-center gap-3 rounded-full bg-slate-100 px-3 py-2 text-sm font-semibold text-slate-700">
              <div class="flex h-9 w-9 items-center justify-center overflow-hidden rounded-full bg-white text-slate-400 shadow-sm">
                @if (authService.userAvatarUrl()) {
                  <img
                    [src]="authService.userAvatarUrl()!"
                    [alt]="authService.userName() || '會員頭像'"
                    class="h-full w-full object-cover"
                  />
                } @else {
                  <i class="pi pi-user text-sm"></i>
                }
              </div>
              <span>{{ authService.userName() || '會員' }}</span>
            </div>
          } @else {
            <button
              pButton
              icon="pi pi-user"
              routerLink="/login"
              class="p-button-rounded p-button-text text-slate-600 hover:text-indigo-600 p-1 md:p-3"
            ></button>
          }

          @if (authService.isAuthenticated()) {
            <button
              pButton
              icon="pi pi-list"
              routerLink="/orders"
              class="p-button-rounded p-button-text text-slate-600 hover:text-indigo-600 p-1 md:p-3"
              title="我的訂單"
            ></button>
          }

          @if (authService.isAdmin()) {
            <button
              pButton
              icon="pi pi-shield"
              routerLink="/admin"
              class="p-button-rounded p-button-text text-slate-600 hover:text-indigo-600 p-1 md:p-3"
              title="前往 Admin 管理頁"
            ></button>
          }

          <div class="relative inline-flex items-center">
            <button
              pButton
              icon="pi pi-shopping-cart"
              (click)="onCartClick()"
              class="p-button-rounded p-button-text text-slate-600 hover:text-indigo-600 p-1 md:p-3"
            ></button>
            @if (cartService.totalCount() > 0) {
              <p-badge
                [value]="cartService.totalCount().toString()"
                severity="danger"
                class="absolute -top-1 -right-1 scale-75 md:scale-100"
              ></p-badge>
            }
          </div>

          @if (authService.isAuthenticated()) {
            <button
              pButton
              icon="pi pi-sign-out"
              [loading]="isLoggingOut()"
              (click)="logout()"
              class="hidden md:inline-flex p-button-rounded p-button-text text-slate-600 hover:text-rose-600"
            ></button>
          }
        </div>
      </div>
    </nav>
  `,
})
export class NavbarComponent {
  cartService = inject(CartService);
  authService = inject(AuthService);
  private readonly router = inject(Router);
  isLoggingOut = signal(false);

  logout(): void {
    this.isLoggingOut.set(true);
    this.authService.logout().pipe(
      finalize(() => this.isLoggingOut.set(false))
    ).subscribe({
      next: () => {
        void this.router.navigateByUrl('/');
      },
    });
  }

  onCartClick(): void {
    if (!this.authService.isAuthenticated()) {
      void this.router.navigate(['/login']);
      return;
    }

    this.cartService.open();
  }
}
