import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-register-page',
  standalone: true,
  imports: [FormsModule, RouterLink],
  templateUrl: './register.page.html',
})
export class RegisterPageComponent {
  name = signal('');
  email = signal('');
  phone = signal('');
  password = signal('');
  confirmPassword = signal('');
  isSubmitting = signal(false);
  error = signal('');
  success = signal('');

  constructor(
    private readonly authService: AuthService,
    private readonly router: Router,
  ) {}

  async submit(): Promise<void> {
    this.error.set('');
    this.success.set('');

    if (!this.name() || !this.email() || !this.phone() || !this.password()) {
      this.error.set('請完整填寫所有欄位。');
      return;
    }
    if (this.password() !== this.confirmPassword()) {
      this.error.set('密碼與確認密碼不一致。');
      return;
    }

    this.isSubmitting.set(true);
    try {
      await this.authService.register({
        name: this.name(),
        email: this.email(),
        phone: this.phone(),
        password: this.password(),
      });
      this.success.set('註冊成功，請登入。');
      await this.router.navigate(['/auth/login']);
    } catch (err) {
      this.error.set(err instanceof Error ? err.message : '註冊失敗，請稍後再試。');
    } finally {
      this.isSubmitting.set(false);
    }
  }
}
