import { Component, effect, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzBadgeModule } from 'ng-zorro-antd/badge';

import { AuthService } from '../../core/services/auth.service';
import { AccountService } from '../../core/services/account.service';
import { CartService } from '../../core/services/cart.service';
import { GetMeResponse } from '../../core/models/api-models';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [RouterLink, FormsModule, NzInputModule, NzIconModule, NzBadgeModule],
  templateUrl: './header.component.html',
})
export class HeaderComponent {
  searchKeyword = signal('');
  cartCount = signal(0);
  me = signal<GetMeResponse | null>(null);

  constructor(
    private readonly authService: AuthService,
    private readonly accountService: AccountService,
    private readonly cartService: CartService,
    private readonly router: Router
  ) {
    this.cartCount = this.cartService.cartCount;
    effect(() => {
      if (this.authService.authState()) {
        void this.loadMe();
        void this.loadCartCount();
      } else {
        this.me.set(null);
        this.cartService.cartCount.set(0);
      }
    });
  }

  isAuthenticated(): boolean {
    return this.authService.authenticated();
  }

  isSeller(): boolean {
    return this.me()?.isSeller ?? false;
  }

  logout(): void {
    this.authService.logout();
    this.me.set(null);
    this.cartService.cartCount.set(0);
    void this.router.navigate(['/auth/login']);
  }

  onSearch(): void {
    const keyword = this.searchKeyword().trim();
    if (keyword) {
      void this.router.navigate(['/products'], { queryParams: { q: keyword } });
    }
  }

  private async loadMe(): Promise<void> {
    try {
      const me = await this.accountService.getMe();
      this.me.set(me);
    } catch {
      this.me.set(null);
    }
  }

  private async loadCartCount(): Promise<void> {
    const userId = this.authService.getUserId();
    if (!userId) {
      this.cartService.cartCount.set(0);
      return;
    }
    try {
      await this.cartService.getCart(userId);
    } catch {
      this.cartService.cartCount.set(0);
    }
  }
}
