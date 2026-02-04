import { Component, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';

import { AccountService } from '../../core/services/account.service';
import { GetMeResponse } from '../../core/models/api-models';
import { SectionComponent } from '../../shared/layout/section/section.component';

@Component({
  selector: 'app-account-page',
  standalone: true,
  imports: [SectionComponent, DatePipe, RouterLink],
  templateUrl: './account.page.html',
})
export class AccountPageComponent {
  me = signal<GetMeResponse | null>(null);
  isLoading = signal(true);

  constructor(private readonly accountService: AccountService) {
    void this.load();
  }

  private async load(): Promise<void> {
    this.me.set(await this.accountService.getMe());
    this.isLoading.set(false);
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
}
