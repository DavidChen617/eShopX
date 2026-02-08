import { Component, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';

@Component({
  selector: 'app-line-pay-fail-page',
  standalone: true,
  imports: [RouterLink],
  template: `
    <section class="flex min-h-[60dvh] items-center justify-center px-4 py-12">
      <div class="w-full max-w-md rounded-2xl border border-rose-200 bg-rose-50 p-6 text-center">
        <div class="text-lg font-semibold text-rose-900">付款失敗</div>
        <div class="mt-2 text-sm text-rose-700">請稍後再試或聯絡客服。</div>
        <div class="mt-4 text-left text-xs text-rose-800">
          <div>Reason: {{ reason() || '-' }}</div>
          <div>Transaction ID: {{ transactionId() || '-' }}</div>
          <div>Order ID: {{ orderId() || '-' }}</div>
        </div>
        <a
          class="mt-5 inline-flex rounded-lg bg-rose-600 px-4 py-2 text-sm font-semibold text-white"
          routerLink="/"
        >
          回首頁
        </a>
      </div>
    </section>
  `,
})
export class LinePayFailPageComponent {
  reason = signal('');
  transactionId = signal('');
  orderId = signal('');

  constructor(route: ActivatedRoute) {
    this.reason.set(route.snapshot.queryParamMap.get('reason') ?? '');
    this.transactionId.set(route.snapshot.queryParamMap.get('transactionId') ?? '');
    this.orderId.set(route.snapshot.queryParamMap.get('orderId') ?? '');
  }
}
