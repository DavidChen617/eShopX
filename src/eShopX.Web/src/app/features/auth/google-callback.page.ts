import { Component, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-google-callback-page',
  standalone: true,
  template: `
    <section class="flex min-h-[60dvh] items-center justify-center px-4 py-12">
      <div class="text-center">
        <div class="text-lg font-semibold text-neutral-900">Google 登入中...</div>
        @if (error()) {
          <div class="mt-3 text-sm text-rose-500">{{ error() }}</div>
        }
      </div>
    </section>
  `,
})
export class GoogleCallbackPageComponent {
  error = signal('');

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly authService: AuthService,
  ) {
    this.route.queryParams.subscribe((params) => {
      const code = params['code'];
      const state = params['state'];

      if (!code || !state) {
        this.error.set('缺少授權資訊，請重新登入。');
        return;
      }

      try {
        this.authService.exchangeGoogleCode(code, state).subscribe({
          next: (result) => {
            this.authService.storeAuthResult(result);
            this.authService.clearTempGoogleState();
            void this.router.navigate(['/']);
          },
          error: () => {
            this.authService.clearTempGoogleState();
            this.error.set('Google 登入失敗，請稍後再試。');
          },
        });
      } catch (err) {
        this.authService.clearTempGoogleState();
        this.error.set('Google 登入驗證失敗，請重新登入。');
      }
    });
  }
}
