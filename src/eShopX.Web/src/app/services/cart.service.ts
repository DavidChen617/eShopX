import { HttpClient } from '@angular/common/http';
import { Injectable, computed, effect, inject, signal } from '@angular/core';
import { catchError, map, Observable, of, switchMap, tap } from 'rxjs';
import { ApiResponse, Cart, CartItem } from '../models/api.models';
import { AuthService } from './auth.service';

type CartApiItem = Omit<CartItem, 'selected'>;
type CartApiResponse = {
  cartId: string;
  userId: string;
  items: CartApiItem[];
};

@Injectable({
  providedIn: 'root',
})
export class CartService {
  private readonly http = inject(HttpClient);
  private readonly authService = inject(AuthService);
  private readonly baseUrl = '/api/v1/cart';

  readonly isDrawerVisible = signal(false);
  readonly isLoading = signal(false);
  readonly isMutating = signal(false);

  readonly cart = signal<Cart>({
    cartId: '',
    userId: '',
    items: [],
  });

  readonly totalCount = computed(() =>
    this.cart().items.filter((i) => i.selected).reduce((sum, item) => sum + item.quantity, 0)
  );

  readonly totalAmount = computed(() =>
    this.cart().items.filter((i) => i.selected).reduce((sum, item) => sum + item.unitPrice * item.quantity, 0)
  );

  readonly isAllSelected = computed(() =>
    this.cart().items.length > 0 && this.cart().items.every((i) => i.selected)
  );

  constructor() {
    effect(() => {
      if (this.authService.isAuthenticated()) {
        this.loadCart().subscribe();
        return;
      }

      this.cart.set(this.emptyCart());
    });
  }

  open(): void {
    this.isDrawerVisible.set(true);
  }

  close(): void {
    this.isDrawerVisible.set(false);
  }

  loadCart(): Observable<Cart> {
    if (!this.authService.isAuthenticated()) {
      this.cart.set(this.emptyCart());
      return of(this.cart());
    }

    this.isLoading.set(true);

    return this.http.get<ApiResponse<CartApiResponse>>(`${this.baseUrl}`).pipe(
      map((response) => this.toCart(response.data, this.cart().items)),
      tap((cart) => this.cart.set(cart)),
      catchError(() => {
        this.cart.set(this.emptyCart());
        return of(this.cart());
      }),
      tap(() => this.isLoading.set(false))
    );
  }

  addItem(skuId: string, quantity = 1): Observable<void> {
    if (!this.authService.isAuthenticated()) {
      return of(void 0);
    }

    this.isMutating.set(true);

    return this.http.post<void>(`${this.baseUrl}/items`, { skuId, quantity }).pipe(
      switchMap(() => this.loadCart()),
      tap(() => this.open()),
      map(() => void 0),
      catchError(() => of(void 0)),
      tap(() => this.isMutating.set(false))
    );
  }

  toggleItemSelection(skuId: string): void {
    this.cart.update((prev) => ({
      ...prev,
      items: prev.items.map((item) =>
        item.skuId === skuId ? { ...item, selected: !item.selected } : item
      ),
    }));
  }

  toggleAll(selected: boolean): void {
    this.cart.update((prev) => ({
      ...prev,
      items: prev.items.map((item) => ({ ...item, selected })),
    }));
  }

  updateQuantity(skuId: string, delta: number): void {
    const currentItem = this.cart().items.find((item) => item.skuId === skuId);
    if (!currentItem) {
      return;
    }

    const nextQuantity = Math.max(1, currentItem.quantity + delta);

    this.isMutating.set(true);
    this.http.put<void>(`${this.baseUrl}/items/${skuId}`, { quantity: nextQuantity }).pipe(
      switchMap(() => this.loadCart()),
      catchError(() => of(this.cart())),
      tap(() => this.isMutating.set(false))
    ).subscribe();
  }

  removeItem(skuId: string): void {
    this.isMutating.set(true);
    this.http.delete<void>(`${this.baseUrl}/items/${skuId}`).pipe(
      switchMap(() => this.loadCart()),
      catchError(() => of(this.cart())),
      tap(() => this.isMutating.set(false))
    ).subscribe();
  }

  clearCart(): void {
    this.isMutating.set(true);
    this.http.delete<void>(this.baseUrl).pipe(
      tap(() => this.cart.set(this.emptyCart())),
      catchError(() => of(void 0)),
      tap(() => this.isMutating.set(false))
    ).subscribe();
  }

  private toCart(response: CartApiResponse, previousItems: CartItem[]): Cart {
    return {
      cartId: response.cartId,
      userId: response.userId,
      items: response.items.map((item) => ({
        ...item,
        selected: previousItems.find((previous) => previous.skuId === item.skuId)?.selected ?? true,
      })),
    };
  }

  private emptyCart(): Cart {
    return {
      cartId: '',
      userId: '',
      items: [],
    };
  }
}
