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

  private resolveSellerStatus(me: GetMeResponse | null): 'Pending' | 'Approved' | 'Rejected' | null {
    if (!me) return null;
    if (me.isSeller || me.sellerApprovedAt || me.sellerStatus === 'Approved') return 'Approved';
    if (me.sellerStatus === 'Pending') return 'Pending';
    if (me.sellerStatus === 'Rejected') return 'Rejected';
    return null;
  }

  sellerStatusLabel(me: GetMeResponse | null): string {
    const status = this.resolveSellerStatus(me);
    if (me?.isAdmin) return '管理員';
    if (status === 'Pending') return '申請中';
    if (status === 'Approved') return '已通過';
    if (status === 'Rejected') return '已拒絕';
    return '尚未申請';
  }
}
