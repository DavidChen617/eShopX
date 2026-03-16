import { Component, OnDestroy, signal } from '@angular/core';

import { GetMeResponse } from '../../core/models/api-models';
import { BannerManageItem } from '../../core/models/home-models';
import { AccountService } from '../../core/services/account.service';
import { HomepageService } from '../../core/services/homepage.service';
import { SectionComponent } from '../../shared/layout/section/section.component';
import { ButtonComponent } from '../../shared/ui/button/button.component';

@Component({
  selector: 'app-admin-banners-page',
  standalone: true,
  imports: [SectionComponent, ButtonComponent],
  templateUrl: './admin-banners.page.html',
})
export class AdminBannersPageComponent implements OnDestroy {
  me = signal<GetMeResponse | null>(null);
  items = signal<BannerManageItem[]>([]);
  isLoading = signal(true);
  isSaving = signal(false);
  error = signal<string | null>(null);
  formError = signal<string | null>(null);

  editingId = signal<string | null>(null);
  title = signal('');
  imageUrl = signal('');
  link = signal('');
  sortOrder = signal(0);
  isActive = signal(true);
  startsAt = signal('');
  endsAt = signal('');
  selectedFile = signal<File | null>(null);
  selectedFilePreviewUrl = signal<string | null>(null);

  constructor(
    private readonly accountService: AccountService,
    private readonly homepageService: HomepageService,
  ) {
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
      if (!me.isAdmin) return;
      await this.reloadBanners();
      this.startCreate();
    } catch (err) {
      this.error.set(err instanceof Error ? err.message : '載入失敗，請稍後再試。');
    } finally {
      this.isLoading.set(false);
    }
  }

  startCreate(): void {
    this.editingId.set(null);
    this.title.set('');
    this.imageUrl.set('');
    this.link.set('');
    this.sortOrder.set(this.items().length);
    this.isActive.set(true);
    this.startsAt.set('');
    this.endsAt.set('');
    this.selectedFile.set(null);
    this.resetPreviewUrl(null);
    this.formError.set(null);
  }

  startEdit(item: BannerManageItem): void {
    this.editingId.set(item.id);
    this.title.set(item.title);
    this.imageUrl.set(item.imageUrl);
    this.link.set(item.link);
    this.sortOrder.set(item.sortOrder ?? 0);
    this.isActive.set(item.isActive ?? true);
    this.startsAt.set(this.toDateTimeLocal(item.startsAt ?? null));
    this.endsAt.set(this.toDateTimeLocal(item.endsAt ?? null));
    this.selectedFile.set(null);
    this.resetPreviewUrl(null);
    this.formError.set(null);
  }

  onFileChange(event: Event): void {
    const input = event.target as HTMLInputElement | null;
    const file = input?.files?.[0] ?? null;
    this.selectedFile.set(file);
    this.resetPreviewUrl(file);
  }

  async save(): Promise<void> {
    if (this.isSaving()) return;

    const title = this.title().trim();
    const imageUrl = this.imageUrl().trim();
    const link = this.link().trim();
    const sortOrder = Number(this.sortOrder());
    const startsAt = this.toIso(this.startsAt());
    const endsAt = this.toIso(this.endsAt());

    if (!title) {
      this.formError.set('請輸入標題。');
      return;
    }
    if (!link) {
      this.formError.set('請輸入連結。');
      return;
    }
    if (!this.selectedFile() && !imageUrl) {
      this.formError.set('請提供圖片 URL 或上傳圖片檔。');
      return;
    }
    if (startsAt && endsAt && new Date(endsAt).getTime() < new Date(startsAt).getTime()) {
      this.formError.set('結束時間不能早於開始時間。');
      return;
    }

    this.formError.set(null);
    this.isSaving.set(true);
    try {
      const payload = {
        title,
        imageUrl: imageUrl || null,
        file: this.selectedFile(),
        link,
        sortOrder: Number.isFinite(sortOrder) ? sortOrder : 0,
        startsAt,
        endsAt,
      };

      const id = this.editingId();
      if (id) {
        await this.homepageService.updateBanner(id, {
          ...payload,
          isActive: this.isActive(),
        });
      } else {
        await this.homepageService.createBanner(payload);
      }

      await this.reloadBanners();
      this.startCreate();
    } catch (err) {
      this.formError.set(err instanceof Error ? err.message : '儲存失敗，請稍後再試。');
    } finally {
      this.isSaving.set(false);
    }
  }

  async remove(item: BannerManageItem): Promise<void> {
    if (this.isSaving()) return;
    if (!window.confirm(`確定要刪除 Banner「${item.title}」嗎？`)) return;
    this.isSaving.set(true);
    this.formError.set(null);
    try {
      await this.homepageService.deleteBanner(item.id);
      this.items.update((list) => list.filter((x) => x.id !== item.id));
      if (this.editingId() === item.id) {
        this.startCreate();
      }
    } catch (err) {
      this.formError.set(err instanceof Error ? err.message : '刪除失敗，請稍後再試。');
    } finally {
      this.isSaving.set(false);
    }
  }

  private async reloadBanners(): Promise<void> {
    const items = await this.homepageService.getBannersForAdmin();
    this.items.set(items);
  }

  private toDateTimeLocal(value: string | null): string {
    if (!value) return '';
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return '';
    const pad = (n: number) => String(n).padStart(2, '0');
    const y = date.getFullYear();
    const m = pad(date.getMonth() + 1);
    const d = pad(date.getDate());
    const h = pad(date.getHours());
    const mm = pad(date.getMinutes());
    return `${y}-${m}-${d}T${h}:${mm}`;
  }

  private toIso(value: string): string | null {
    if (!value) return null;
    const date = new Date(value);
    return Number.isNaN(date.getTime()) ? null : date.toISOString();
  }

  private resetPreviewUrl(file: File | null): void {
    const current = this.selectedFilePreviewUrl();
    if (current) {
      URL.revokeObjectURL(current);
    }
    this.selectedFilePreviewUrl.set(file ? URL.createObjectURL(file) : null);
  }

  ngOnDestroy(): void {
    const current = this.selectedFilePreviewUrl();
    if (current) {
      URL.revokeObjectURL(current);
    }
  }
}
