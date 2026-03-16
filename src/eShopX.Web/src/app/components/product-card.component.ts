import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { BadgeModule } from 'primeng/badge';

export interface Product {
  id: number | string;
  name: string;
  price: number;
  image: string;
  category: string;
  createdAt: string; // ISO 字串
}

@Component({
  selector: 'app-product-card',
  standalone: true,
  imports: [CommonModule, RouterLink, ButtonModule, BadgeModule],
  template: `
    <div
      [routerLink]="['/product', product.id]"
      class="group relative bg-white rounded-2xl overflow-hidden hover:shadow-2xl transition-all duration-500 transform hover:-translate-y-1 cursor-pointer"
    >
      <!-- Product Image -->
      <div class="aspect-square relative overflow-hidden bg-slate-100">
        <img
          [src]="product.image"
          [alt]="product.name"
          class="w-full h-full object-cover group-hover:scale-110 transition-transform duration-700"
        />
        <!-- New Badge (Logic: within 24 hours) -->
        @if (isNew) {
          <span
            class="absolute top-3 left-3 bg-indigo-600 text-white text-[10px] font-bold px-2 py-1 rounded-full uppercase tracking-wider shadow-lg"
          >
            New
          </span>
        }

        <!-- Quick Action Overlay -->
        <div
          class="absolute inset-x-0 bottom-0 p-4 bg-gradient-to-t from-black/20 to-transparent translate-y-full group-hover:translate-y-0 transition-transform duration-300 flex justify-center gap-2"
        >
          <button
            pButton
            icon="pi pi-eye"
            class="p-button-rounded bg-white/90 text-slate-900 border-none hover:bg-white"
          ></button>
        </div>
      </div>

      <!-- Content -->
      <div class="p-5">
        <span class="text-xs font-semibold text-indigo-600 uppercase tracking-widest">{{
          product.category
        }}</span>
        <h3 class="mt-1 text-lg font-bold text-slate-900 line-clamp-1">{{
          product.name
        }}</h3>
        <div class="mt-3 flex items-center justify-between">
          <span class="text-xl font-black text-slate-900">{{
            product.price | currency : 'USD'
          }}</span>
        </div>
      </div>
    </div>
  `,
  styles: [
    `
      :host {
        display: block;
      }
    `,
  ],
})
export class ProductCardComponent {
  @Input() product!: Product;

  get isNew(): boolean {
    if (!this.product?.createdAt) return false;
    const createdDate = new Date(this.product.createdAt).getTime();
    const now = new Date().getTime();
    const diffInMilliseconds = now - createdDate;
    const diffInHours = diffInMilliseconds / (1000 * 60 * 60);
    return diffInHours <= 24 && diffInHours >= 0;
  }
}
