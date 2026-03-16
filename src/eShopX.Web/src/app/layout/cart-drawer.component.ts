import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { DrawerModule } from 'primeng/drawer';
import { ButtonModule } from 'primeng/button';
import { CheckboxModule } from 'primeng/checkbox';
import { FormsModule } from '@angular/forms';
import { CartService } from '../services/cart.service';

@Component({
  selector: 'app-cart-drawer',
  standalone: true,
  imports: [CommonModule, RouterLink, DrawerModule, ButtonModule, CheckboxModule, FormsModule],
  template: `
    <p-drawer
      [(visible)]="cartService.isDrawerVisible"
      position="right"
      styleClass="w-full md:w-[480px] border-l-0 shadow-2xl"
      header="您的購物車"
    >
      <div class="flex flex-col h-full p-2 md:p-4">
        <!-- Header Actions: Select All -->
        @if (cartService.cart().items.length > 0) {
          <div class="flex items-center justify-between mb-6 pb-4 border-b border-slate-100">
            <div class="flex items-center gap-3 cursor-pointer" (click)="cartService.toggleAll(!cartService.isAllSelected())">
              <p-checkbox 
                [binary]="true" 
                [ngModel]="cartService.isAllSelected()"
                (click)="$event.stopPropagation()"
                (ngModelChange)="cartService.toggleAll($event)">
              </p-checkbox>
              <span class="text-sm font-bold text-slate-700">全選 ({{ cartService.cart().items.length }})</span>
            </div>
            <button 
              class="text-xs font-bold text-rose-500 hover:text-rose-600"
              (click)="cartService.clearCart()">
              清空購物車
            </button>
          </div>
        }

        <!-- Cart Items -->
        <div class="flex-1 overflow-y-auto pr-2 custom-scrollbar">
          @if (cartService.cart().items.length === 0) {
            <div class="flex flex-col items-center justify-center h-64 text-slate-400">
              <i class="pi pi-shopping-cart text-6xl mb-4"></i>
              <p>購物車是空的</p>
            </div>
          } @else {
            <div class="space-y-6">
              @for (item of cartService.cart().items; track item.skuId) {
                <div class="flex items-center gap-4 group">
                  <!-- Checkbox -->
                  <p-checkbox 
                    [binary]="true" 
                    [(ngModel)]="item.selected"
                    (ngModelChange)="cartService.toggleItemSelection(item.skuId)">
                  </p-checkbox>

                  <!-- Image -->
                  <div class="w-20 h-24 bg-slate-100 rounded-xl overflow-hidden shrink-0 shadow-sm relative">
                    <img [src]="item.primaryImageUrl" class="w-full h-full object-cover" [class.grayscale]="!item.selected" />
                    @if (!item.selected) {
                      <div class="absolute inset-0 bg-white/40"></div>
                    }
                  </div>

                  <!-- Details -->
                  <div class="flex-1 min-w-0 flex flex-col justify-between h-24 py-1">
                    <div>
                      <div class="flex justify-between items-start gap-2">
                        <h4 class="font-bold text-slate-900 truncate tracking-tight" [class.text-slate-400]="!item.selected">
                          {{ item.productName }}
                        </h4>
                        <button
                          (click)="cartService.removeItem(item.skuId)"
                          class="text-slate-300 hover:text-rose-500 transition-colors"
                        >
                          <i class="pi pi-trash text-xs"></i>
                        </button>
                      </div>
                      <p class="text-[10px] text-slate-400 mt-1 uppercase tracking-widest">
                        {{ item.color }} / {{ item.sizeName }}
                      </p>
                    </div>

                    <div class="flex items-center justify-between">
                      <!-- Quantity -->
                      <div class="flex items-center bg-slate-50 border border-slate-100 rounded-lg px-2 py-1 gap-3" [class.opacity-50]="!item.selected">
                        <button
                          (click)="cartService.updateQuantity(item.skuId, -1)"
                          [disabled]="!item.selected || cartService.isMutating()"
                          class="w-6 h-6 flex items-center justify-center text-slate-400 hover:text-indigo-600 transition-colors disabled:cursor-not-allowed"
                        >
                          <i class="pi pi-minus text-[8px]"></i>
                        </button>
                        <span class="text-xs font-bold w-4 text-center text-slate-700">{{ item.quantity }}</span>
                        <button
                          (click)="cartService.updateQuantity(item.skuId, 1)"
                          [disabled]="!item.selected || cartService.isMutating()"
                          class="w-6 h-6 flex items-center justify-center text-slate-400 hover:text-indigo-600 transition-colors disabled:cursor-not-allowed"
                        >
                          <i class="pi pi-plus text-[8px]"></i>
                        </button>
                      </div>
                      <span class="font-black text-slate-900" [class.text-slate-400]="!item.selected">
                        {{ item.unitPrice * item.quantity | currency : 'USD' }}
                      </span>
                    </div>
                  </div>
                </div>
              }
            </div>
          }
        </div>

        <!-- Footer -->
        <div class="pt-8 border-t border-slate-100 space-y-4 bg-white">
          <div class="flex items-center justify-between text-lg font-bold text-slate-900">
            <div class="flex flex-col">
              <span class="text-sm text-slate-500 font-normal">已選擇 {{ cartService.totalCount() }} 件商品</span>
              <span>合計</span>
            </div>
            <span class="text-3xl font-black text-indigo-600">{{
              cartService.totalAmount() | currency : 'USD'
            }}</span>
          </div>
          <a
            routerLink="/checkout"
            (click)="cartService.close()"
            [class.pointer-events-none]="cartService.totalCount() === 0"
            [class.opacity-50]="cartService.totalCount() === 0"
            class="flex items-center justify-center gap-2 w-full bg-indigo-600 text-white rounded-2xl py-4 font-bold shadow-xl hover:bg-indigo-700 transition-all"
          >
            <i class="pi pi-credit-card"></i>
            {{ cartService.totalCount() > 0 ? '前往結帳 (' + cartService.totalCount() + ')' : '請選擇商品' }}
          </a>
          <button
            pButton
            (click)="cartService.close()"
            label="繼續購物"
            class="w-full p-button-text text-slate-400 font-bold hover:text-slate-600"
          ></button>
        </div>
      </div>
    </p-drawer>
  `,
  styles: [
    `
      @reference "tailwindcss";
      :host ::ng-deep .p-drawer-header {
        @apply px-6 pb-2 pt-6;
      }
      :host ::ng-deep .p-drawer-content {
        @apply flex flex-col h-full overflow-hidden;
      }
      :host ::ng-deep .p-checkbox-box {
        @apply rounded-md border-slate-300;
      }
      :host ::ng-deep .p-checkbox-checked .p-checkbox-box {
        @apply bg-indigo-600 border-indigo-600;
      }
      .custom-scrollbar::-webkit-scrollbar {
        width: 4px;
      }
      .custom-scrollbar::-webkit-scrollbar-thumb {
        @apply bg-slate-200 rounded-full;
      }
    `,
  ],
})
export class CartDrawerComponent {
  cartService = inject(CartService);
}
