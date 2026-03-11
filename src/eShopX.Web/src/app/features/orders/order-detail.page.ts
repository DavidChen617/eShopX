import { Component, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';

import { OrdersService } from '../../core/services/orders.service';
import { GetOrderResponse } from '../../core/models/api-models';
import { ReviewService } from '../../core/services/review.service';
import { SectionComponent } from '../../shared/layout/section/section.component';
import { ButtonComponent } from '../../shared/ui/button/button.component';

@Component({
  selector: 'app-order-detail-page',
  standalone: true,
  imports: [SectionComponent, DatePipe, FormsModule, ButtonComponent],
  templateUrl: './order-detail.page.html',
})
export class OrderDetailPageComponent {
  order = signal<GetOrderResponse | null>(null);
  isLoading = signal(true);
  reviewRatingDrafts = signal<Record<string, number>>({});
  reviewContentDrafts = signal<Record<string, string>>({});
  reviewSubmitting = signal<Record<string, boolean>>({});
  reviewMessage = signal<Record<string, string>>({});

  constructor(
    private readonly route: ActivatedRoute,
    private readonly ordersService: OrdersService,
    private readonly reviewService: ReviewService,
  ) {
    const orderId = this.route.snapshot.paramMap.get('id') ?? '';
    void this.load(orderId);
  }

  private async load(_orderId: string): Promise<void> {
    this.order.set(await this.ordersService.getOrderDetail(_orderId));
    this.isLoading.set(false);
  }

  getDraftRating(orderItemId: string): number {
    return this.reviewRatingDrafts()[orderItemId] ?? 5;
  }

  getDraftContent(orderItemId: string): string {
    return this.reviewContentDrafts()[orderItemId] ?? '';
  }

  setDraftRating(orderItemId: string, value: string): void {
    this.reviewRatingDrafts.update((state) => ({ ...state, [orderItemId]: Number(value) }));
  }

  setDraftContent(orderItemId: string, value: string): void {
    this.reviewContentDrafts.update((state) => ({ ...state, [orderItemId]: value }));
  }

  getReviewMessage(orderItemId: string): string {
    return this.reviewMessage()[orderItemId] ?? '';
  }

  isSubmitting(orderItemId: string): boolean {
    return this.reviewSubmitting()[orderItemId] ?? false;
  }

  async submitReview(orderId: string, orderItemId: string): Promise<void> {
    const rating = this.getDraftRating(orderItemId);
    const content = this.getDraftContent(orderItemId).trim();

    this.reviewSubmitting.update((state) => ({ ...state, [orderItemId]: true }));
    this.reviewMessage.update((state) => ({ ...state, [orderItemId]: '' }));

    try {
      await this.reviewService.createReview(orderId, {
        orderItemId,
        rating,
        content: content || null,
      });
      this.reviewMessage.update((state) => ({ ...state, [orderItemId]: '評論已送出' }));
    } catch {
      this.reviewMessage.update((state) => ({ ...state, [orderItemId]: '送出失敗，請稍後再試' }));
    } finally {
      this.reviewSubmitting.update((state) => ({ ...state, [orderItemId]: false }));
    }
  }
}
