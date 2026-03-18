import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { catchError, map, Observable, of, tap } from 'rxjs';
import { ApiResponse } from '../models/api.models';
import { apiUrl } from '../shared/api.config';

export type AdminOrderStatus = 'PendingPayment' | 'Paid' | 'Shipped' | 'Completed';

export interface AdminOrderItem {
  orderId: string;
  userId: string;
  status: AdminOrderStatus;
  totalAmount: number;
  createdAt: string;
  shipment?: ShipmentSummary;
  logisticsId?: string;
  logisticsSubType?: string;
}

interface ShipmentSummary {
  logisticsSubType: string;
  logisticsId: string;
  receiverName: string;
  receiverPhone: string;
  storeName?: string;
  address?: string;
}

interface GetAdminOrdersResponse {
  items: AdminOrderItem[];
  totalCount: number;
  page: number;
  pageSize: number;
}

interface PrintLabelRequest {
  logisticsId: string;
  logisticsSubType: string;
}

interface ShipOrderResponse {
  logisticsId: string;
  logisticsSubType: string;
}

@Injectable({
  providedIn: 'root',
})
export class AdminOrderService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = apiUrl('/v1/admin/orders');

  orders = signal<AdminOrderItem[]>([]);
  totalCount = signal(0);
  currentPage = signal(1);
  pageSize = signal(10);
  currentStatus = signal<AdminOrderStatus | null>(null);

  loadOrders(page = 1, pageSize = 10, status?: AdminOrderStatus | null): Observable<AdminOrderItem[]> {
    let params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);

    if (status) {
      params = params.set('status', status);
    }

    return this.http.get<ApiResponse<GetAdminOrdersResponse>>(this.baseUrl, { params }).pipe(
      map((response) => ({
        ...response.data,
        items: response.data.items.map((order) => this.normalizeOrder(order)),
      })),
      tap((response) => {
        this.orders.set(response.items);
        this.totalCount.set(response.totalCount);
        this.currentPage.set(response.page);
        this.pageSize.set(response.pageSize);
        this.currentStatus.set(status ?? null);
      }),
      map((response) => response.items),
      catchError(() => {
        this.orders.set([]);
        this.totalCount.set(0);
        return of([]);
      })
    );
  }

  ship(orderId: string): Observable<ShipOrderResponse> {
    return this.http.post<ShipOrderResponse | ApiResponse<ShipOrderResponse>>(`${this.baseUrl}/${orderId}/ship`, {}).pipe(
      map((response) => ('data' in response ? response.data : response)),
      tap((shipment) =>
        this.orders.update((orders) =>
          orders.map((order) =>
            order.orderId === orderId
              ? { ...order, status: 'Shipped', logisticsId: shipment.logisticsId, logisticsSubType: shipment.logisticsSubType }
              : order
          )
        )
      )
    );
  }

  complete(orderId: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${orderId}/complete`, {}).pipe(
      tap(() =>
        this.orders.update((orders) =>
          orders.map((order) =>
            order.orderId === orderId ? { ...order, status: 'Completed' } : order
          )
        )
      )
    );
  }

  printLabel(request: PrintLabelRequest): Observable<string> {
    return this.http.post(`${this.baseUrl}/print-label`, request, { responseType: 'text' });
  }

  private normalizeOrder(order: AdminOrderItem): AdminOrderItem {
    return {
      ...order,
      logisticsId: order.shipment?.logisticsId,
      logisticsSubType: order.shipment?.logisticsSubType,
    };
  }
}
