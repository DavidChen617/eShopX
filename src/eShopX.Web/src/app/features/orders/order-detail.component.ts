import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { OrderService } from '../../services/order.service';
import { OrderResponse, OrderStatus } from '../../models/api.models';

@Component({
  selector: 'app-order-detail',
  imports: [DecimalPipe, RouterLink, ProgressSpinnerModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="max-w-2xl mx-auto px-4 py-12">
      @if (isLoading()) {
        <div class="flex justify-center py-20">
          <p-progressSpinner strokeWidth="3" styleClass="w-10 h-10"></p-progressSpinner>
        </div>
      } @else if (!order()) {
        <div class="text-center py-20 text-slate-400">
          <p>找不到此訂單</p>
          <a routerLink="/orders" class="mt-4 inline-block text-indigo-600 font-bold">回到訂單列表</a>
        </div>
      } @else {
        @if (paymentSuccess()) {
          <div class="bg-emerald-50 border border-emerald-200 rounded-2xl p-5 mb-6 flex items-center gap-4">
            <i class="pi pi-check-circle text-3xl text-emerald-500"></i>
            <div>
              <p class="font-black text-emerald-800">付款成功！</p>
              <p class="text-sm text-emerald-600">我們已收到您的付款，訂單正在處理中。</p>
            </div>
          </div>
        }

        <div class="flex items-center justify-between mb-6">
          <div>
            <p class="text-xs text-slate-400 uppercase tracking-widest mb-1">訂單編號</p>
            <h1 class="font-mono font-black text-slate-900">{{ order()!.orderId }}</h1>
          </div>
          <span class="rounded-full px-4 py-1.5 text-sm font-bold" [class]="statusClass(order()!.status)">
            {{ statusLabel(order()!.status) }}
          </span>
        </div>

        <!-- 商品清單 -->
        <section class="bg-white rounded-2xl border border-slate-200 p-6 mb-4">
          <h2 class="font-black text-slate-800 mb-4">商品明細</h2>
          <div class="space-y-4">
            @for (item of order()!.items; track item.itemId) {
              <div class="flex justify-between items-start">
                <div>
                  <p class="font-bold text-slate-800">{{ item.productName }}</p>
                  <p class="text-xs text-slate-400 mt-0.5 uppercase tracking-wider">
                    {{ item.color }} / {{ item.size }} × {{ item.quantity }}
                  </p>
                </div>
                <span class="font-black text-slate-700">TWD {{ item.totalPrice | number }}</span>
              </div>
            }
          </div>
        </section>

        <!-- 總計 -->
        <section class="bg-white rounded-2xl border border-slate-200 p-6 mb-6">
          <div class="flex justify-between items-center">
            <span class="text-slate-600">訂單總額</span>
            <span class="text-2xl font-black text-indigo-600">TWD {{ order()!.totalAmount | number }}</span>
          </div>
          <p class="text-xs text-slate-400 mt-2">{{ formatDate(order()!.createdAt) }}</p>
        </section>

        <a routerLink="/orders" class="text-sm font-bold text-indigo-600 hover:text-indigo-800">
          ← 回到訂單列表
        </a>
      }
    </div>
  `,
})
export class OrderDetailComponent implements OnInit {
  private readonly orderService = inject(OrderService);
  private readonly route = inject(ActivatedRoute);

  readonly order = signal<OrderResponse | null>(null);
  readonly isLoading = signal(true);
  readonly paymentSuccess = signal(false);

  ngOnInit(): void {
    const orderId = this.route.snapshot.paramMap.get('orderId')!;
    this.paymentSuccess.set(this.route.snapshot.queryParamMap.get('payment') === 'success');

    this.orderService.getOrderById(orderId).subscribe({
      next: (order) => {
        this.order.set(order);
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
