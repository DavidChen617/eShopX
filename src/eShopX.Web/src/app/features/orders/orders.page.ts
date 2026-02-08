import { Component, signal } from '@angular/core';
import { DatePipe } from '@angular/common';

import { RouterLink } from '@angular/router';

import { OrdersService } from '../../core/services/orders.service';
import { AuthService } from '../../core/services/auth.service';
import { GetUserOrderResponse } from '../../core/models/api-models';
import { SectionComponent } from '../../shared/layout/section/section.component';

@Component({
  selector: 'app-orders-page',
  standalone: true,
  imports: [SectionComponent, RouterLink, DatePipe],
  templateUrl: './orders.page.html',
})
export class OrdersPageComponent {
  orders = signal<GetUserOrderResponse | null>(null);
  isLoading = signal(true);

  constructor(
    private readonly ordersService: OrdersService,
    private readonly authService: AuthService,
  ) {
    void this.load();
  }

  private async load(): Promise<void> {
    const userId = this.authService.getUserId();
    if (!userId) {
      this.isLoading.set(false);
      return;
    }
    this.orders.set(await this.ordersService.getOrders(userId));
    this.isLoading.set(false);
  }
}
