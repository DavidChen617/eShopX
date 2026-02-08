import { Injectable } from '@angular/core';

import { ApiService } from './api.service';
import { GetOrderResponse, GetUserOrderResponse } from '../models/api-models';

@Injectable({ providedIn: 'root' })
export class OrdersService {
  constructor(private readonly api: ApiService) {}

  getOrders(userId: string, page = 1, pageSize = 10): Promise<GetUserOrderResponse> {
    return this.api.get<GetUserOrderResponse>('/api/orders', {
      userId,
      page,
      pageSize,
    });
  }

  getOrderDetail(orderId: string): Promise<GetOrderResponse> {
    return this.api.get<GetOrderResponse>(`/api/orders/${orderId}`);
  }
}
