import { Component, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';

@Component({
  selector: 'app-paypal-success-page',
  standalone: true,
  imports: [RouterLink],
  template: `
    <section class="flex min-h-[60dvh] items-center justify-center px-4 py-12">
      <div class="w-full max-w-md rounded-2xl border border-emerald-200 bg-emerald-50 p-6 text-center">
        <div class="text-lg font-semibold text-emerald-900">PayPal 付款成功</div>
        <div class="mt-2 text-sm text-emerald-700">感謝您的付款。</div>
        <div class="mt-4 text-left text-xs text-emerald-800">
          <div>Order ID: {{ orderId() || '-' }}</div>
        </div>
        <a class="mt-5 inline-flex rounded-lg bg-emerald-600 px-4 py-2 text-sm font-semibold text-white" routerLink="/">
          回首頁
        </a>
      </div>
    </section>
  `,
})
export class PayPalSuccessPageComponent {
  orderId = signal('');

  constructor(route: ActivatedRoute) {
    this.orderId.set(route.snapshot.queryParamMap.get('orderId') ?? '');
  }
}
