import { HttpErrorResponse } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { FloatLabel } from 'primeng/floatlabel';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { finalize } from 'rxjs';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    ButtonModule,
    InputTextModule,
    PasswordModule,
    CardModule,
    FloatLabel,
  ],
  template: `
    <div class="min-h-[80vh] flex items-center justify-center px-4 py-12">
      <div class="w-full max-w-md">
        <div class="text-center mb-8">
          <div
            class="inline-flex w-16 h-16 bg-indigo-600 rounded-2xl items-center justify-center text-white shadow-xl mb-4"
          >
            <i class="pi pi-shopping-bag text-3xl"></i>
          </div>
          <h1 class="text-3xl font-black text-slate-900 tracking-tight">歡迎回來</h1>
          <p class="mt-2 text-slate-500">登入您的 eShopX 帳戶，繼續探索極致生活風格</p>
        </div>

        <p-card class="shadow-2xl border-none">
          <form class="space-y-6 flex flex-col pt-4" (ngSubmit)="submit()" #loginForm="ngForm">
            <p-floatlabel>
              <input
                id="email"
                name="email"
                type="email"
                pInputText
                [(ngModel)]="email"
                required
                email
                autocomplete="email"
                class="w-full border-slate-200 rounded-xl py-3 px-4"
              />
              <label for="email">電子郵件 (Email)</label>
            </p-floatlabel>

            <p-floatlabel>
              <p-password
                id="password"
                name="password"
                [(ngModel)]="password"
                [toggleMask]="true"
                [feedback]="false"
                required
                autocomplete="current-password"
                class="w-full"
                inputStyleClass="w-full border-slate-200 rounded-xl py-3 px-4"
              ></p-password>
              <label for="password">密碼 (Password)</label>
            </p-floatlabel>

            @if (errorMessage()) {
              <div class="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
                {{ errorMessage() }}
              </div>
            }

            <button
              pButton
              type="submit"
              label="立即登入"
              icon="pi pi-sign-in"
              [loading]="isSubmitting()"
              [disabled]="isSubmitting() || loginForm.invalid || isGoogleLoading() || isLineLoading()"
              class="w-full p-button-lg bg-indigo-600 text-white border-none rounded-xl py-4 font-bold shadow-lg hover:bg-indigo-700 transition-all"
            ></button>

            <div class="flex items-center gap-3 text-xs uppercase tracking-[0.3em] text-slate-400">
              <div class="h-px flex-1 bg-slate-200"></div>
              <span>或使用</span>
              <div class="h-px flex-1 bg-slate-200"></div>
            </div>

            <button
              type="button"
              (click)="loginWithGoogle()"
              [disabled]="isGoogleLoading() || isLineLoading() || isSubmitting()"
              class="flex h-12 w-full items-center justify-center gap-3 rounded-full border border-[#747775] bg-white px-5 text-sm font-semibold text-[#1f1f1f] transition hover:shadow-md disabled:cursor-not-allowed disabled:opacity-70"
            >
              <svg width="20" height="20" viewBox="0 0 20 20" aria-hidden="true">
                <path d="M19.6 10.2273C19.6 9.51818 19.5364 8.83636 19.4182 8.18182H10V12.05H15.3818C15.15 13.3 14.4455 14.3591 13.3864 15.0682V17.5773H16.6182C18.5091 15.8364 19.6 13.2727 19.6 10.2273Z" fill="#4285F4"/>
                <path d="M10 20C12.7 20 14.9636 19.1045 16.6181 17.5773L13.3863 15.0682C12.4909 15.6682 11.3454 16.0227 10 16.0227C7.39545 16.0227 5.19091 14.2636 4.40455 11.9H1.06364V14.4909C2.70909 17.7591 6.09091 20 10 20Z" fill="#34A853"/>
                <path d="M4.40454 11.9C4.20454 11.3 4.09091 10.6591 4.09091 10C4.09091 9.34091 4.20454 8.7 4.40454 8.1V5.50909H1.06363C0.386364 6.85909 0 8.38636 0 10C0 11.6136 0.386364 13.1409 1.06363 14.4909L4.40454 11.9Z" fill="#FBBC04"/>
                <path d="M10 3.97727C11.4682 3.97727 12.7864 4.48182 13.8227 5.47273L16.6909 2.60455C14.9591 0.990909 12.6955 0 10 0C6.09091 0 2.70909 2.24091 1.06363 5.50909L4.40454 8.1C5.19091 5.73636 7.39545 3.97727 10 3.97727Z" fill="#E94235"/>
              </svg>
              <span>{{ isGoogleLoading() ? 'Google 跳轉中...' : '使用 Google 登入' }}</span>
            </button>

            <button
              type="button"
              (click)="loginWithLine()"
              [disabled]="isGoogleLoading() || isLineLoading() || isSubmitting()"
              class="flex h-12 w-full items-center justify-center gap-3 rounded-full bg-[#06C755] px-5 text-sm font-semibold text-white transition hover:bg-[#05b34c] disabled:cursor-not-allowed disabled:opacity-70"
            >
              <img src="/auth/line-btn-base.png" alt="LINE" class="h-5 w-5 rounded-sm bg-white object-contain" />
              <span>{{ isLineLoading() ? 'LINE 跳轉中...' : '使用 LINE 登入' }}</span>
            </button>
          </form>

          <div class="mt-8 text-center text-sm text-slate-500">
            還沒有帳號嗎？
            <a
              routerLink="/register"
              class="font-bold text-indigo-600 hover:text-indigo-700 transition-colors"
              >立即免費註冊</a
            >
          </div>
        </p-card>
      </div>
    </div>
  `,
  styles: [
    `
      @reference "tailwindcss";
      :host ::ng-deep .p-card {
        @apply rounded-3xl overflow-hidden border-none;
      }
      :host ::ng-deep .p-card-body {
        @apply p-8;
      }
      :host ::ng-deep .p-inputtext:focus {
        @apply ring-2 ring-indigo-600/20 border-indigo-600;
      }
    `,
  ],
})
export class LoginComponent {
  private readonly authService = inject(AuthService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  email = '';
  password = '';
  isSubmitting = signal(false);
  isGoogleLoading = signal(false);
  isLineLoading = signal(false);
  errorMessage = signal('');

  constructor() {
    this.email = this.route.snapshot.queryParamMap.get('email') ?? '';
  }

  submit(): void {
    if (!this.email.trim() || !this.password) {
      this.errorMessage.set('請輸入電子郵件與密碼。');
      return;
    }

    this.errorMessage.set('');
    this.isSubmitting.set(true);

    this.authService.login({
      email: this.email.trim(),
      password: this.password,
    }).pipe(
      finalize(() => this.isSubmitting.set(false))
    ).subscribe({
      next: () => {
        void this.router.navigateByUrl('/');
      },
      error: (error: unknown) => {
        this.errorMessage.set(this.resolveErrorMessage(error));
      },
    });
  }

  async loginWithGoogle(): Promise<void> {
    this.errorMessage.set('');
    this.isGoogleLoading.set(true);

    try {
      await this.authService.loginWithGoogle();
    } catch (error) {
      this.errorMessage.set(this.resolveErrorMessage(error));
      this.isGoogleLoading.set(false);
    }
  }

  async loginWithLine(): Promise<void> {
    this.errorMessage.set('');
    this.isLineLoading.set(true);

    try {
      await this.authService.loginWithLine();
    } catch (error) {
      this.errorMessage.set(this.resolveErrorMessage(error));
      this.isLineLoading.set(false);
    }
  }

  private resolveErrorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      const apiError = error.error as { problem?: { detail?: string; title?: string } } | undefined;
      return apiError?.problem?.detail || apiError?.problem?.title || '登入失敗，請檢查帳號密碼。';
    }

    if (error instanceof Error) {
      return error.message || '登入失敗，請稍後再試。';
    }

    return '登入失敗，請稍後再試。';
  }
}
