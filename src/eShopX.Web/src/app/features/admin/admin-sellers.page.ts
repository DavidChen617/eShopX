import { Component, signal } from '@angular/core';
import { DatePipe } from '@angular/common';

import { AccountService } from '../../core/services/account.service';
import { SellerAdminService } from '../../core/services/seller-admin.service';
import { GetMeResponse, PendingSellerItem } from '../../core/models/api-models';
import { SectionComponent } from '../../shared/layout/section/section.component';
import { ButtonComponent } from '../../shared/ui/button/button.component';

@Component({
  selector: 'app-admin-sellers-page',
  standalone: true,
  imports: [SectionComponent, ButtonComponent, DatePipe],
  templateUrl: './admin-sellers.page.html',
})
export class AdminSellersPageComponent {
  me = signal<GetMeResponse | null>(null);
  items = signal<PendingSellerItem[]>([]);
  isLoading = signal(true);
  error = signal<string | null>(null);
  actionLoading = signal<string | null>(null);
  rejectReason = signal<Record<string, string>>({});

  constructor(
    private readonly accountService: AccountService,
    private readonly sellerAdminService: SellerAdminService
  ) {
    void this.load();
  }

  isAdmin(): boolean {
    return this.me()?.isAdmin ?? false;
  }

  setReason(userId: string, value: string): void {
    this.rejectReason.update((current) => ({ ...current, [userId]: value }));
  }

  getReason(userId: string): string {
    return this.rejectReason()[userId] ?? '';
  }

  async approve(userId: string): Promise<void> {
    if (this.actionLoading()) {
      return;
    }
    this.actionLoading.set(userId);
    try {
      await this.sellerAdminService.approve(userId);
      this.items.update((list) => list.filter((item) => item.userId !== userId));
    } catch (err) {
      this.error.set(err instanceof Error ? err.message : '核准失敗，請稍後再試。');
    } finally {
      this.actionLoading.set(null);
    }
  }

  async reject(userId: string): Promise<void> {
    if (this.actionLoading()) {
      return;
    }
    const reason = this.getReason(userId).trim();
    if (!reason) {
      this.error.set('請輸入拒絕原因。');
      return;
    }
    this.actionLoading.set(userId);
    try {
      await this.sellerAdminService.reject(userId, reason);
      this.items.update((list) => list.filter((item) => item.userId !== userId));
    } catch (err) {
      this.error.set(err instanceof Error ? err.message : '拒絕失敗，請稍後再試。');
    } finally {
      this.actionLoading.set(null);
    }
  }

  private async load(): Promise<void> {
    this.error.set(null);
    this.isLoading.set(true);
    try {
      const me = await this.accountService.getMe();
      this.me.set(me);
      if (!me.isAdmin) {
        this.items.set([]);
        return;
      }
      const result = await this.sellerAdminService.getPending();
      this.items.set(result.items ?? []);
    } catch (err) {
      this.error.set(err instanceof Error ? err.message : '載入失敗，請稍後再試。');
    } finally {
      this.isLoading.set(false);
    }
  }
}
