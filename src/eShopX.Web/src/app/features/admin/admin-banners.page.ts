import { Component, signal } from '@angular/core';

import { GetMeResponse } from '../../core/models/api-models';
import { AccountService } from '../../core/services/account.service';
import { SectionComponent } from '../../shared/layout/section/section.component';

@Component({
  selector: 'app-admin-banners-page',
  standalone: true,
  imports: [SectionComponent],
  templateUrl: './admin-banners.page.html',
})
export class AdminBannersPageComponent {
  me = signal<GetMeResponse | null>(null);
  isLoading = signal(true);
  error = signal<string | null>(null);

  constructor(private readonly accountService: AccountService) {
    void this.load();
  }

  isAdmin(): boolean {
    return this.me()?.isAdmin ?? false;
  }

  private async load(): Promise<void> {
    this.error.set(null);
    this.isLoading.set(true);
    try {
      const me = await this.accountService.getMe();
      this.me.set(me);
    } catch (err) {
      this.error.set(err instanceof Error ? err.message : '載入失敗，請稍後再試。');
    } finally {
      this.isLoading.set(false);
    }
  }
}
