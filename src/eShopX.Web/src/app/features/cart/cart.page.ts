import { Component, signal } from '@angular/core';

import { CartService } from '../../core/services/cart.service';
import { AuthService } from '../../core/services/auth.service';
import { GetCartResponse } from '../../core/models/api-models';
import { SectionComponent } from '../../shared/layout/section/section.component';

@Component({
  selector: 'app-cart-page',
  standalone: true,
  imports: [SectionComponent],
  templateUrl: './cart.page.html',
})
export class CartPageComponent {
  cart = signal<GetCartResponse | null>(null);
  isLoading = signal(true);
  payError = signal('');
  actionError = signal('');
  isUpdating = signal(false);

  constructor(
    private readonly cartService: CartService,
    private readonly authService: AuthService,
  ) {
    void this.load();
  }

  private async load(): Promise<void> {
    const userId = this.authService.getUserId();
    if (!userId) {
      this.isLoading.set(false);
      return;
    }
    this.cart.set(await this.cartService.getCart(userId));
    this.isLoading.set(false);
  }

  private getUserId(): string | null {
    return this.authService.getUserId();
  }

  async changeQuantity(productId: string, nextQty: number): Promise<void> {
    const userId = this.getUserId();
    if (!userId) return;
    if (nextQty < 1) {
      await this.removeItem(productId);
      return;
    }
    this.isUpdating.set(true);
    this.actionError.set('');
    try {
      await this.cartService.updateQuantity(userId, productId, nextQty);
      await this.load();
    } catch (err) {
      this.actionError.set(err instanceof Error ? err.message : '更新數量失敗');
    } finally {
      this.isUpdating.set(false);
    }
  }

  async removeItem(productId: string): Promise<void> {
    const userId = this.getUserId();
    if (!userId) return;
    this.isUpdating.set(true);
    this.actionError.set('');
    try {
      await this.cartService.removeItem(userId, productId);
      await this.load();
    } catch (err) {
      this.actionError.set(err instanceof Error ? err.message : '移除失敗');
    } finally {
      this.isUpdating.set(false);
    }
  }

  async clearCart(): Promise<void> {
    const userId = this.getUserId();
    if (!userId) return;
    this.isUpdating.set(true);
    this.actionError.set('');
    try {
      await this.cartService.clearCart(userId);
      await this.load();
    } catch (err) {
      this.actionError.set(err instanceof Error ? err.message : '清空失敗');
    } finally {
      this.isUpdating.set(false);
    }
  }

  async payWithPayPal(): Promise<void> {
    const cart = this.cart();
    if (!cart || (cart.items?.length ?? 0) === 0) {
      this.payError.set('Your cart is empty.');
      return;
    }

    try {
      this.payError.set('');
      const amount = cart.totalAmount ?? 0;
      if (amount <= 0) {
        this.payError.set('Invalid amount.');
        return;
      }
      const orderId = `PP-${Date.now()}`;
      await this.authService.createPayPalOrder(amount, 'TWD', orderId);
    } catch (err) {
      this.payError.set('PayPal 付款失敗，請稍後再試。');
    }
  }

  async payWithLinePay(): Promise<void> {
    const cart = this.cart();
    if (!cart || (cart.items?.length ?? 0) === 0) {
      this.payError.set('Your cart is empty.');
      return;
    }

    try {
      this.payError.set('');
      const items = cart.items.map((item) => ({
        id: item.productId,
        name: item.productName,
        quantity: item.quantity,
        price: Math.round(item.unitPrice),
      }));
      const amount = items.reduce((sum, item) => sum + item.price * item.quantity, 0);
      if (amount <= 0) {
        this.payError.set('Invalid amount.');
        return;
      }

      await this.authService.createLinePayOrder(amount, 'TWD', items);
    } catch (err) {
      this.payError.set('LINE Pay 付款失敗，請稍後再試。');
    }
  }
}
