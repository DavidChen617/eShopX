import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login-page',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './login.page.html',
})
export class LoginPageComponent {
  email = signal('');
  password = signal('');
  isSubmitting = signal(false);
  error = signal('');

  constructor(
    private readonly authService: AuthService,
    private readonly router: Router,
  ) {}

  async submit(): Promise<void> {
    this.error.set('');

    if (!this.email() || !this.password()) {
      this.error.set('Please enter email and password.');
      return;
    }

    this.isSubmitting.set(true);
    try {
      await this.authService.login({
        email: this.email(),
        password: this.password(),
      });
      await this.router.navigate(['/']);
    } catch (err) {
      this.error.set(err instanceof Error ? err.message : 'Login failed. Please try again.');
    } finally {
      this.isSubmitting.set(false);
    }
  }

  loginWithGoogle(): void {
    this.authService.loginWithGoogle();
  }

  loginWithLine(): void {
    this.authService.loginWithLine();
  }

  async testPayPal(): Promise<void> {
    try {
      await this.authService.testPayPal();
    } catch (err) {
      this.error.set('PayPal 測試失敗，請檢查後端設定。');
    }
  }
}
