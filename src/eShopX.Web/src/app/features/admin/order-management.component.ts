import { CommonModule } from '@angular/common';
import { Component, computed, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { TableModule, TablePageEvent } from 'primeng/table';
import { ToastModule } from 'primeng/toast';
import { SearchInputComponent } from '../../components/search-input.component';
import { AdminOrderItem, AdminOrderService, AdminOrderStatus } from '../../services/admin-order.service';

@Component({
  selector: 'app-order-management',
  standalone: true,
  imports: [CommonModule, FormsModule, TableModule, ButtonModule, SelectModule, ToastModule, SearchInputComponent],
  providers: [MessageService],
  template: `
    <div class="overflow-hidden rounded-3xl border border-slate-200 bg-white shadow-sm">
      <p-toast position="top-right"></p-toast>

      <div class="flex flex-col gap-4 border-b border-slate-100 p-6 md:flex-row md:items-center md:justify-between">
        <div>
          <h1 class="text-2xl font-black text-slate-900">訂單管理</h1>
          <p class="text-sm text-slate-500">處理已付款訂單的出貨、完成訂單與託運單列印。</p>
        </div>

        <div class="flex flex-col gap-3 md:flex-row">
          <div class="w-full md:w-72">
            <app-search-input
              [value]="keyword"
              placeholder="搜尋訂單編號 / 會員"
              (valueChange)="keyword = $event"
            ></app-search-input>
          </div>

          <p-select
            [(ngModel)]="selectedStatus"
            [options]="statusOptions"
            optionLabel="label"
            optionValue="value"
            placeholder="全部狀態"
            styleClass="min-w-48"
            (ngModelChange)="onStatusChange()"
          ></p-select>
        </div>
      </div>

      <p-table
        [value]="filteredOrders()"
        [paginator]="true"
        [rows]="rows()"
        [first]="first()"
        [totalRecords]="totalRecords()"
        [rowsPerPageOptions]="[10, 20, 50]"
        [lazy]="true"
        (onPage)="onPageChange($event)"
        currentPageReportTemplate="{first} - {last} / 共 {totalRecords} 筆訂單"
        [showCurrentPageReport]="true"
        styleClass="p-datatable-gridlines"
      >
        <ng-template pTemplate="header">
          <tr class="bg-slate-50/50 text-xs uppercase tracking-widest text-slate-400">
            <th class="px-6 py-4">訂單編號</th>
            <th class="px-6 py-4 text-center">會員</th>
            <th class="px-6 py-4 text-center">狀態</th>
            <th class="px-6 py-4 text-center">金額</th>
            <th class="px-6 py-4 text-center">建立時間</th>
            <th class="px-6 py-4 text-center">物流</th>
            <th class="px-6 py-4 text-right">操作</th>
          </tr>
        </ng-template>

        <ng-template pTemplate="body" let-order>
          <tr class="border-b border-slate-100 transition-colors hover:bg-slate-50/80 last:border-none">
            <td class="px-6 py-4">
              <div class="font-black text-slate-900">{{ order.orderId }}</div>
              <div class="mt-1 text-[10px] font-mono text-slate-400">{{ order.logisticsId || '-' }}</div>
            </td>
            <td class="px-6 py-4 text-center">
              <span class="text-sm font-bold text-slate-700">{{ order.userId }}</span>
            </td>
            <td class="px-6 py-4 text-center">
              <span class="rounded-full px-3 py-1 text-xs font-bold" [class]="statusClass(order.status)">
                {{ statusLabel(order.status) }}
              </span>
            </td>
            <td class="px-6 py-4 text-center">
              <span class="font-mono font-bold text-slate-700">TWD {{ order.totalAmount }}</span>
            </td>
            <td class="px-6 py-4 text-center">
              <span class="text-sm text-slate-600">{{ formatDate(order.createdAt) }}</span>
            </td>
            <td class="px-6 py-4 text-center">
              <span class="text-sm font-medium text-slate-600">{{ order.logisticsSubType || '-' }}</span>
            </td>
            <td class="px-6 py-4">
              <div class="flex justify-end gap-2">
                <button
                  pButton
                  type="button"
                  label="列印託運單"
                  icon="pi pi-print"
                  (click)="printLabel(order)"
                  [disabled]="!order.logisticsId || !order.logisticsSubType"
                  class="rounded-xl border border-slate-200 bg-white px-4 py-2 text-sm font-bold text-slate-700"
                ></button>

                @if (order.status === 'Paid') {
                  <button
                    pButton
                    type="button"
                    label="出貨"
                    icon="pi pi-send"
                    (click)="ship(order.orderId)"
                    class="rounded-xl border-none bg-indigo-600 px-4 py-2 text-sm font-bold text-white"
                  ></button>
                }

                @if (order.status === 'Shipped') {
                  <button
                    pButton
                    type="button"
                    label="完成訂單"
                    icon="pi pi-check"
                    (click)="complete(order.orderId)"
                    class="rounded-xl border-none bg-emerald-500 px-4 py-2 text-sm font-bold text-slate-950"
                  ></button>
                }
              </div>
            </td>
          </tr>
        </ng-template>
      </p-table>
    </div>
  `,
  styles: [`
    @reference "tailwindcss";
    :host ::ng-deep .p-datatable {
      @apply border-none;
    }
    :host ::ng-deep .p-datatable .p-datatable-thead > tr > th {
      @apply border-none bg-slate-50/50 font-black text-[10px] text-slate-500;
    }
    :host ::ng-deep .p-datatable .p-datatable-tbody > tr > td {
      @apply border-none py-4;
    }
    :host ::ng-deep .p-select {
      @apply border-slate-200;
    }
    :host ::ng-deep .p-toast .p-toast-message {
      @apply rounded-2xl border border-slate-200 bg-white shadow-xl;
    }
    :host ::ng-deep .p-toast .p-toast-message-content {
      @apply items-start gap-3 px-4 py-4;
    }
    :host ::ng-deep .p-paginator {
      @apply border-none border-t border-slate-100 bg-white px-4 py-3;
    }
    :host ::ng-deep .p-paginator .p-paginator-page,
    :host ::ng-deep .p-paginator .p-paginator-prev,
    :host ::ng-deep .p-paginator .p-paginator-next,
    :host ::ng-deep .p-paginator .p-paginator-first,
    :host ::ng-deep .p-paginator .p-paginator-last {
      @apply h-10 min-w-10 rounded-xl text-slate-500 transition-colors;
    }
    :host ::ng-deep .p-paginator .p-paginator-page.p-paginator-page-selected {
      @apply bg-indigo-600 font-bold text-white;
    }
  `],
})
export class OrderManagementComponent implements OnInit {
  private adminOrderService = inject(AdminOrderService);
  private messageService = inject(MessageService);

  keyword = '';
  selectedStatus: AdminOrderStatus | null = null;

  totalRecords = computed(() => this.adminOrderService.totalCount());
  rows = computed(() => this.adminOrderService.pageSize());
  first = computed(() => (this.adminOrderService.currentPage() - 1) * this.adminOrderService.pageSize());

  statusOptions = [
    { label: '待付款', value: 'PendingPayment' as AdminOrderStatus },
    { label: '已付款', value: 'Paid' as AdminOrderStatus },
    { label: '已出貨', value: 'Shipped' as AdminOrderStatus },
    { label: '已完成', value: 'Completed' as AdminOrderStatus },
  ];

  filteredOrders = computed(() => {
    const keyword = this.keyword.trim().toLowerCase();

    return this.adminOrderService.orders().filter((order) =>
      !keyword ||
      order.orderId.toLowerCase().includes(keyword) ||
      order.userId.toLowerCase().includes(keyword)
    );
  });

  ngOnInit() {
    this.adminOrderService.loadOrders().subscribe();
  }

  onStatusChange() {
    this.adminOrderService.loadOrders(1, this.rows(), this.selectedStatus).subscribe();
  }

  onPageChange(event: TablePageEvent) {
    const pageSize = event.rows || this.rows();
    const page = Math.floor((event.first || 0) / pageSize) + 1;
    this.adminOrderService.loadOrders(page, pageSize, this.selectedStatus).subscribe();
  }

  statusLabel(status: AdminOrderStatus): string {
    switch (status) {
      case 'PendingPayment':
        return '待付款';
      case 'Paid':
        return '已付款';
      case 'Shipped':
        return '已出貨';
      case 'Completed':
        return '已完成';
    }
  }

  statusClass(status: AdminOrderStatus): string {
    switch (status) {
      case 'PendingPayment':
        return 'bg-slate-100 text-slate-600';
      case 'Paid':
        return 'bg-sky-100 text-sky-700';
      case 'Shipped':
        return 'bg-amber-100 text-amber-700';
      case 'Completed':
        return 'bg-emerald-100 text-emerald-700';
    }
  }

  formatDate(value: string): string {
    return new Intl.DateTimeFormat('zh-TW', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
    }).format(new Date(value));
  }

  ship(orderId: string) {
    this.adminOrderService.ship(orderId).subscribe({
      next: (shipment) => {
        this.messageService.add({
          severity: 'success',
          summary: '訂單已出貨',
          detail: `${orderId} / ${shipment.logisticsSubType}`,
          life: 2200,
        });
      },
      error: () => {
        this.messageService.add({
          severity: 'error',
          summary: '訂單出貨失敗',
          detail: '請稍後再試，或確認後端 API 是否可用。',
          life: 2600,
        });
      },
    });
  }

  complete(orderId: string) {
    this.adminOrderService.complete(orderId).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: '訂單已完成',
          detail: `${orderId} 已標記為完成`,
          life: 2200,
        });
      },
      error: () => {
        this.messageService.add({
          severity: 'error',
          summary: '完成訂單失敗',
          detail: '請稍後再試，或確認後端 API 是否可用。',
          life: 2600,
        });
      },
    });
  }

  printLabel(order: AdminOrderItem) {
    if (!order.logisticsId || !order.logisticsSubType) {
      this.messageService.add({
        severity: 'warn',
        summary: '尚未有託運單資料',
        detail: '請先確認物流單號與物流類型是否已建立。',
        life: 2200,
      });
      return;
    }

    this.adminOrderService.printLabel({
      logisticsId: order.logisticsId,
      logisticsSubType: order.logisticsSubType,
    }).subscribe({
      next: (html) => {
        const popup = window.open('', '_blank', 'width=900,height=700,scrollbars=yes');
        if (!popup) {
          this.messageService.add({
            severity: 'warn',
            summary: '請允許彈出視窗',
            detail: '瀏覽器封鎖了彈出視窗，請允許後重試。',
            life: 2600,
          });
          return;
        }
        popup.document.open();
        popup.document.write(html);
        popup.document.close();
      },
      error: () => {
        this.messageService.add({
          severity: 'error',
          summary: '列印託運單失敗',
          detail: '請稍後再試，或確認後端 API 是否可用。',
          life: 2600,
        });
      },
    });
  }
}
