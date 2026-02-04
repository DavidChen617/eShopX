import { Component, signal } from '@angular/core';
import { DatePipe } from '@angular/common';

import { AccountService } from '../../core/services/account.service';
import { ApplyForSellerResponse, GetMeResponse } from '../../core/models/api-models';
import { SectionComponent } from '../../shared/layout/section/section.component';
import { ButtonComponent } from '../../shared/ui/button/button.component';

@Component({
  selector: 'app-account-settings-page',
  standalone: true,
  imports: [SectionComponent, ButtonComponent, DatePipe],
  templateUrl: './account-settings.page.html',
})
export class AccountSettingsPageComponent {
  me = signal<GetMeResponse | null>(null);
  isLoading = signal(true);
  applyLoading = signal(false);
  applyResult = signal<ApplyForSellerResponse | null>(null);
  applyError = signal<string | null>(null);
  isSaving = signal(false);
  saveMessage = signal<string | null>(null);
  saveError = signal<string | null>(null);
  avatarUploading = signal(false);
  avatarError = signal<string | null>(null);
  name = signal('');
  phone = signal('');
  address = signal('');

  constructor(private readonly accountService: AccountService) {
    void this.load();
  }

  async load(): Promise<void> {
    const me = await this.accountService.getMe();
    this.me.set(me);
    this.name.set(me.name);
    this.phone.set(me.phone);
    this.address.set(me.address ?? '');
    this.isLoading.set(false);
  }

  async applyForSeller(): Promise<void> {
    if (this.applyLoading()) {
      return;
    }

    this.applyLoading.set(true);
    this.applyError.set(null);
    try {
      const result = await this.accountService.applyForSeller();
      this.applyResult.set(result);
      this.me.update((current) =>
        current
          ? {
              ...current,
              sellerStatus: result.status,
              sellerAppliedAt: result.appliedAt,
            }
          : current
      );
    } catch (error) {
      const message =
        error instanceof Error ? error.message : '申請失敗，請稍後再試。';
      this.applyError.set(message);
    } finally {
      this.applyLoading.set(false);
    }
  }

  private resolveSellerStatus(me: GetMeResponse | null): number | null {
    if (!me) return null;
    if (me.isSeller || me.sellerApprovedAt || me.sellerStatus === 1) return 1;
    if (me.sellerStatus === 0) return 0;
    if (me.sellerStatus === 2) return 2;
    return null;
  }

  sellerStatusLabel(me: GetMeResponse | null): string {
    const status = this.resolveSellerStatus(me);
    if (me?.isAdmin) return '管理員';
    if (status === 0) return '申請中';
    if (status === 1) return '已通過';
    if (status === 2) return '已拒絕';
    return '尚未申請';
  }

  sellerStatusValue(me: GetMeResponse | null): number | null {
    return this.resolveSellerStatus(me);
  }

  async saveProfile(): Promise<void> {
    if (this.isSaving()) return;
    this.saveMessage.set(null);
    this.saveError.set(null);
    this.isSaving.set(true);
    try {
      await this.accountService.updateMe({
        name: this.name().trim(),
        phone: this.phone().trim(),
        address: this.address().trim() || null,
      });
      this.saveMessage.set('已更新資料');
      const me = await this.accountService.getMe();
      this.me.set(me);
    } catch (err) {
      this.saveError.set(err instanceof Error ? err.message : '更新失敗，請稍後再試。');
    } finally {
      this.isSaving.set(false);
    }
  }

  async uploadAvatar(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement | null;
    const file = input?.files?.[0] ?? null;
    if (!file) return;

    this.avatarUploading.set(true);
    this.avatarError.set(null);
    try {
      await this.accountService.uploadAvatar(file);
      const me = await this.accountService.getMe();
      this.me.set(me);
    } catch (err) {
      this.avatarError.set(err instanceof Error ? err.message : '上傳失敗，請稍後再試。');
    } finally {
      this.avatarUploading.set(false);
      if (input) input.value = '';
    }
  }

  async removeAvatar(): Promise<void> {
    if (this.avatarUploading()) return;
    this.avatarUploading.set(true);
    this.avatarError.set(null);
    try {
      await this.accountService.deleteAvatar();
      const me = await this.accountService.getMe();
      this.me.set(me);
    } catch (err) {
      this.avatarError.set(err instanceof Error ? err.message : '刪除失敗，請稍後再試。');
    } finally {
      this.avatarUploading.set(false);
    }
  }
}
