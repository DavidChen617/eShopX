import { Injectable, signal } from '@angular/core';

import { ApiService } from './api.service';
import {
  ApproveSellerResponse,
  GetPendingSellersResponse,
  RejectSellerResponse,
} from '../models/api-models';

@Injectable({ providedIn: 'root' })
export class SellerAdminService {
  pendingCount = signal(0);

  constructor(private readonly api: ApiService) {}

  async getPending(): Promise<GetPendingSellersResponse> {
    const result = await this.api.get<GetPendingSellersResponse>('/api/sellers/pending');
    this.pendingCount.set(result.items?.length ?? 0);
    return result;
  }

  approve(userId: string): Promise<ApproveSellerResponse> {
    return this.api.post<ApproveSellerResponse, Record<string, never>>(
      `/api/sellers/${userId}/approve`,
      {},
    );
  }

  reject(userId: string, reason: string): Promise<RejectSellerResponse> {
    return this.api.post<RejectSellerResponse, { reason: string }>(
      `/api/sellers/${userId}/reject`,
      { reason },
    );
  }

  decrementPendingCount(): void {
    this.pendingCount.update((count) => Math.max(0, count - 1));
  }

  resetPendingCount(): void {
    this.pendingCount.set(0);
  }
}
