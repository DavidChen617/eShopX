import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { OrderService } from '../../services/order.service';
import { OrderListItem, OrderStatus } from '../../models/api.models';

@Component({
  selector: 'app-orders',
  imports: [DecimalPipe, RouterLink, ProgressSpinnerModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="max-w-2xl mx-auto px-4 py-12">
      <h1 class="text-3xl font-black text-slate-900 mb-8">我的訂單</h1>

      @if (isLoading()) {
        <div class="flex justify-center py-20">
          <p-progressSpinner strokeWidth="3" styleClass="w-10 h-10"></p-progressSpinner>
        </div>
      } @else if (orders().length === 0) {
        <div class="text-center py-20 text-slate-400">
          <i class="pi pi-inbox text-5xl mb-4 block"></i>
          <p>目前沒有訂單</p>
        </div>
      } @else {
        <div class="space-y-4">
          @for (order of orders(); track order.orderId) {
            <a
              [routerLink]="['/orders', order.orderId]"
              class="block bg-white rounded-2xl border border-slate-200 p-5 hover:border-indigo-300 hover:shadow-sm transition-all"
            >
              <div class="flex items-center justify-between mb-3">
                <span class="font-mono text-sm font-bold text-slate-500">{{ order.orderId }}</span>
                <span class="rounded-full px-3 py-1 text-xs font-bold" [class]="statusClass(order.status)">
                  {{ statusLabel(order.status) }}
                </span>
              </div>
              <div class="flex items-center justify-between">
                <span class="text-sm text-slate-400">{{ formatDate(order.createdAt) }}</span>
                <span class="font-black text-slate-900">TWD {{ order.totalAmount | number }}</span>
              </div>
            </a>
          }
        </div>
      }
    </div>
  `,
})
export class OrdersComponent implements OnInit {
  private readonly orderService = inject(OrderService);

  readonly orders = signal<OrderListItem[]>([]);
  readonly isLoading = signal(true);

  ngOnInit(): void {
    this.orderService.getOrders().subscribe({
      next: (res) => {
        this.orders.set(res.items);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false),
    });
  }

  statusLabel(status: OrderStatus): string {
    const map: Record<OrderStatus, string> = {
      PendingPayment: '待付款',
      Paid: '已付款',
      Shipped: '已出貨',
      Completed: '已完成',
    };
    return map[status];
  }

  statusClass(status: OrderStatus): string {
    const map: Record<OrderStatus, string> = {
      PendingPayment: 'bg-slate-100 text-slate-600',
      Paid: 'bg-sky-100 text-sky-700',
      Shipped: 'bg-amber-100 text-amber-700',
      Completed: 'bg-emerald-100 text-emerald-700',
    };
    return map[status];
  }

  formatDate(value: string): string {
    return new Intl.DateTimeFormat('zh-TW', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
    }).format(new Date(value));
  }
}
