import { Injectable } from '@angular/core';

import { ApiService } from './api.service';
import { ApplyForSellerResponse, GetMeResponse, UploadUserAvatarResponse } from '../models/api-models';

@Injectable({ providedIn: 'root' })
export class AccountService {
  constructor(private readonly api: ApiService) {}

  getMe(): Promise<GetMeResponse> {
    return this.api.get<GetMeResponse>('/api/users/me');
  }

  updateMe(request: { name: string; phone: string; address?: string | null }): Promise<void> {
    return this.api.put<void, { name: string; phone: string; address?: string | null }>(
      '/api/users/me',
      request
    );
  }

  uploadAvatar(file: File): Promise<UploadUserAvatarResponse> {
    const form = new FormData();
    form.append('file', file);
    return this.api.post<UploadUserAvatarResponse, FormData>('/api/users/me/avatar', form);
  }

  deleteAvatar(): Promise<void> {
    return this.api.delete<void>('/api/users/me/avatar');
  }

  applyForSeller(): Promise<ApplyForSellerResponse> {
    return this.api.post<ApplyForSellerResponse, Record<string, never>>('/api/sellers/apply', {});
  }
}
