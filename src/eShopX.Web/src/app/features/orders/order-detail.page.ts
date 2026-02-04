import { Component, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ActivatedRoute } from '@angular/router';

import { OrdersService } from '../../core/services/orders.service';
import { GetOrderResponse } from '../../core/models/api-models';
import { SectionComponent } from '../../shared/layout/section/section.component';

@Component({
  selector: 'app-order-detail-page',
  standalone: true,
  imports: [SectionComponent, DatePipe],
  templateUrl: './order-detail.page.html',
})
export class OrderDetailPageComponent {
  order = signal<GetOrderResponse | null>(null);
  isLoading = signal(true);

  constructor(private readonly route: ActivatedRoute, private readonly ordersService: OrdersService) {
    const orderId = this.route.snapshot.paramMap.get('id') ?? '';
    void this.load(orderId);
  }

  private async load(_orderId: string): Promise<void> {
    this.order.set(await this.ordersService.getOrderDetail(_orderId));
    this.isLoading.set(false);
  }
}
