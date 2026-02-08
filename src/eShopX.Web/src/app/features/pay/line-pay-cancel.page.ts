import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-line-pay-cancel-page',
  standalone: true,
  imports: [RouterLink],
  template: `
    <section class="flex min-h-[60dvh] items-center justify-center px-4 py-12">
      <div class="w-full max-w-md rounded-2xl border border-amber-200 bg-amber-50 p-6 text-center">
        <div class="text-lg font-semibold text-amber-900">付款已取消</div>
        <div class="mt-2 text-sm text-amber-700">您已取消付款流程。</div>
        <a
          class="mt-5 inline-flex rounded-lg bg-amber-600 px-4 py-2 text-sm font-semibold text-white"
          routerLink="/"
        >
          回首頁
        </a>
      </div>
    </section>
  `,
})
export class LinePayCancelPageComponent {}
