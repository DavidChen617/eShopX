import { Injectable } from '@angular/core';

import { ApiService } from './api.service';
import {
  ApproveSellerResponse,
  GetPendingSellersResponse,
  RejectSellerResponse,
} from '../models/api-models';

@Injectable({ providedIn: 'root' })
export class SellerAdminService {
  constructor(private readonly api: ApiService) {}

  getPending(): Promise<GetPendingSellersResponse> {
    return this.api.get<GetPendingSellersResponse>('/api/sellers/pending');
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
}
