import { HttpErrorResponse } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-google-callback',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="flex min-h-[60vh] items-center justify-center px-4 py-12">
      <div class="text-center">
        <div class="text-lg font-semibold text-slate-900">Google 登入中...</div>
        @if (errorMessage()) {
          <div class="mt-3 text-sm text-rose-500">{{ errorMessage() }}</div>
        }
      </div>
    </section>
  `,
})
export class GoogleCallbackComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);

  errorMessage = signal('');

  constructor() {
    const code = this.route.snapshot.queryParamMap.get('code');
    const state = this.route.snapshot.queryParamMap.get('state');

    if (!code || !state) {
      this.errorMessage.set('缺少 Google 授權資訊，請重新登入。');
      return;
    }

    this.authService.exchangeGoogleCode(code, state).subscribe({
      next: () => {
        this.authService.clearGoogleState();
        void this.router.navigateByUrl('/');
      },
      error: (error: unknown) => {
        this.authService.clearGoogleState();
        this.errorMessage.set(this.resolveErrorMessage(error, 'Google 登入失敗，請稍後再試。'));
      },
    });
  }

  private resolveErrorMessage(error: unknown, fallback: string): string {
    if (error instanceof HttpErrorResponse) {
      const apiError = error.error as { problem?: { detail?: string; title?: string } } | undefined;
      return apiError?.problem?.detail || apiError?.problem?.title || fallback;
    }

    if (error instanceof Error) {
      return error.message || fallback;
    }

    return fallback;
  }
}
