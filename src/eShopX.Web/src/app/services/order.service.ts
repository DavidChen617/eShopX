import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import {
  ApiResponse,
  CreateOrderRequest,
  CreateOrderResponse,
  GetOrdersResponse,
  LogisticsStatusResponse,
  OrderResponse,
  StartLogisticsRequest,
} from '../models/api.models';
import { apiUrl } from '../shared/api.config';

@Injectable({
  providedIn: 'root',
})
export class OrderService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = apiUrl('/v1/orders');

  startLogisticsSelection(request: StartLogisticsRequest): Observable<string> {
    return this.http
      .post<ApiResponse<string>>(`${this.baseUrl}/logistics/start`, request)
      .pipe(map((r) => r.data));
  }

  getLogisticsStatus(): Observable<LogisticsStatusResponse> {
    return this.http
      .get<ApiResponse<LogisticsStatusResponse>>(`${this.baseUrl}/logistics/status`)
      .pipe(map((r) => r.data));
  }

  createOrder(request: CreateOrderRequest): Observable<CreateOrderResponse> {
    return this.http
      .post<ApiResponse<CreateOrderResponse>>(this.baseUrl, request)
      .pipe(map((r) => r.data));
  }

  getOrders(page = 1, pageSize = 10): Observable<GetOrdersResponse> {
    return this.http
      .get<ApiResponse<GetOrdersResponse>>(this.baseUrl, { params: { page, pageSize } })
      .pipe(map((r) => r.data));
  }

  getOrderById(orderId: string): Observable<OrderResponse> {
    return this.http
      .get<ApiResponse<OrderResponse>>(`${this.baseUrl}/${orderId}`)
      .pipe(map((r) => r.data));
  }
}
