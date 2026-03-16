import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  inject,
  signal,
} from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { InputTextModule } from 'primeng/inputtext';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { CartService } from '../../services/cart.service';
import { OrderService } from '../../services/order.service';
import { LogisticsStatusResponse, PaymentMethod } from '../../models/api.models';

@Component({
  selector: 'app-checkout',
  imports: [
    DecimalPipe,
    ReactiveFormsModule,
    ButtonModule,
    SelectModule,
    InputTextModule,
    ProgressSpinnerModule,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="max-w-lg mx-auto px-4 py-12">
      <h1 class="text-3xl font-black text-slate-900 mb-8">結帳</h1>

      <form [formGroup]="form" class="space-y-6">
        <!-- 收件人資訊 -->
        <section class="bg-white rounded-2xl border border-slate-200 p-6 space-y-4">
          <h2 class="font-black text-slate-800">收件人資訊</h2>

          <div class="flex flex-col gap-1">
            <label for="receiverName" class="text-sm font-bold text-slate-600">姓名</label>
            <input
              pInputText
              id="receiverName"
              formControlName="receiverName"
              placeholder="收件人姓名"
              class="w-full"
            />
          </div>

          <div class="flex flex-col gap-1">
            <label for="receiverPhone" class="text-sm font-bold text-slate-600">電話</label>
            <input
              pInputText
              id="receiverPhone"
              formControlName="receiverPhone"
              placeholder="09xxxxxxxx"
              class="w-full"
            />
          </div>
        </section>

        <!-- 門市選擇 -->
        <section class="bg-white rounded-2xl border border-slate-200 p-6 space-y-4">
          <h2 class="font-black text-slate-800">取貨門市</h2>

          @if (logistics()) {
            <div class="bg-slate-50 rounded-xl p-4 flex items-start justify-between gap-4">
              <div>
                <p class="text-xs text-slate-400 uppercase tracking-widest mb-1">
                  {{ logistics()!.logisticsSubType }}
                </p>
                <p class="font-bold text-slate-800">{{ logistics()!.storeName }}</p>
                @if (logistics()!.address) {
                  <p class="text-sm text-slate-500 mt-0.5">{{ logistics()!.address }}</p>
                }
              </div>
              <button
                type="button"
                (click)="startLogistics()"
                class="text-xs font-bold text-indigo-600 hover:text-indigo-800 shrink-0"
              >
                重新選擇
              </button>
            </div>
          } @else {
            <button
              pButton
              type="button"
              label="選擇取貨門市"
              icon="pi pi-map-marker"
              (click)="startLogistics()"
              [loading]="isLoadingLogistics()"
              [disabled]="form.get('receiverName')!.invalid || form.get('receiverPhone')!.invalid"
              class="w-full bg-slate-900 text-white border-none rounded-xl py-3 font-bold"
            ></button>
            @if (form.get('receiverName')!.invalid || form.get('receiverPhone')!.invalid) {
              <p class="text-xs text-slate-400 text-center">請先填寫收件人資訊</p>
            }
          }
        </section>

        <!-- 付款方式 -->
        <section class="bg-white rounded-2xl border border-slate-200 p-6 space-y-4">
          <h2 class="font-black text-slate-800">付款方式</h2>
          <p-select
            formControlName="paymentMethod"
            [options]="paymentOptions"
            optionLabel="label"
            optionValue="value"
            placeholder="選擇付款方式"
            styleClass="w-full"
          ></p-select>
        </section>

        <!-- 訂單摘要 -->
        <section class="bg-white rounded-2xl border border-slate-200 p-6">
          <div class="flex justify-between items-center">
            <span class="text-slate-600">共 {{ cartService.totalCount() }} 件商品</span>
            <span class="text-2xl font-black text-indigo-600">
              TWD {{ cartService.totalAmount() | number }}
            </span>
          </div>
        </section>

        <!-- 送出 -->
        <button
          pButton
          type="button"
          (click)="submitOrder()"
          [loading]="isSubmitting()"
          [disabled]="!canSubmit()"
          class="w-full bg-indigo-600 text-white border-none rounded-2xl py-4 text-base font-black shadow-xl"
          label="確認下單"
          icon="pi pi-credit-card"
        ></button>

        @if (errorMessage()) {
          <p class="text-center text-sm text-rose-500 font-bold">{{ errorMessage() }}</p>
        }
      </form>
    </div>
  `,
})
export class CheckoutComponent implements OnInit {
  readonly cartService = inject(CartService);
  private readonly orderService = inject(OrderService);
  private readonly fb = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);

  readonly logistics = signal<LogisticsStatusResponse | null>(null);
  readonly isLoadingLogistics = signal(false);
  readonly isSubmitting = signal(false);
  readonly errorMessage = signal('');

  readonly paymentOptions: { label: string; value: PaymentMethod }[] = [
    { label: 'LINE Pay', value: 'LinePay' },
    { label: 'PayPal', value: 'PayPal' },
  ];

  readonly form = this.fb.group({
    receiverName: ['', Validators.required],
    receiverPhone: ['', [Validators.required, Validators.pattern(/^09\d{8}$/)]],
    paymentMethod: [null as PaymentMethod | null, Validators.required],
  });

  readonly canSubmit = signal(false);

  ngOnInit(): void {
    this.form.statusChanges.subscribe(() => {
      this.canSubmit.set(this.form.valid && !!this.logistics());
    });
  }

  startLogistics(): void {
    const { receiverName, receiverPhone } = this.form.value;
    if (!receiverName || !receiverPhone) return;

    this.isLoadingLogistics.set(true);

    this.orderService
      .startLogisticsSelection({
        receiverName,
        receiverPhone,
        goodsAmount: this.cartService.totalAmount(),
      })
      .subscribe({
        next: (html) => {
          const popup = window.open('', '_blank', 'width=900,height=700,scrollbars=yes');
          if (!popup) {
            this.isLoadingLogistics.set(false);
            this.errorMessage.set('請允許瀏覽器開啟彈出視窗，並再試一次');
            return;
          }

          popup.document.open();
          popup.document.write(html);
          popup.document.close();
          this.isLoadingLogistics.set(false);

          let pollTimer: ReturnType<typeof setInterval>;

          const removeListeners = () => {
            clearInterval(pollTimer);
            window.removeEventListener('message', onMessage);
          };

          const onMessage = (event: MessageEvent) => {
            if (event.data?.type === 'ecpay-logistics-done') {
              removeListeners();
              this.fetchLogisticsStatus();
            } else if (event.data?.type === 'ecpay-logistics-error') {
              removeListeners();
              this.errorMessage.set('門市選擇失敗，請稍後再試');
            }
          };

          pollTimer = setInterval(() => {
            if (popup.closed) removeListeners();
          }, 1000);

          window.addEventListener('message', onMessage);
          this.destroyRef.onDestroy(removeListeners);
        },
        error: () => {
          this.isLoadingLogistics.set(false);
          this.errorMessage.set('無法啟動門市選擇，請稍後再試');
        },
      });
  }

  submitOrder(): void {
    if (!this.form.valid || !this.logistics()) return;

    const { receiverName, receiverPhone, paymentMethod } = this.form.value;
    this.isSubmitting.set(true);
    this.errorMessage.set('');

    this.orderService
      .createOrder({
        receiverName: receiverName!,
        receiverPhone: receiverPhone!,
        paymentMethod: paymentMethod!,
      })
      .subscribe({
        next: ({ paymentUrl }) => {
          window.location.href = paymentUrl;
        },
        error: (err) => {
          this.isSubmitting.set(false);
          const detail = err?.error?.problem?.detail;
          this.errorMessage.set(
            detail === 'stock_conflict' ? '部分商品庫存不足，請調整數量' : '下單失敗，請稍後再試',
          );
        },
      });
  }

  private fetchLogisticsStatus(): void {
    this.orderService.getLogisticsStatus().subscribe({
      next: (status) => {
        if (status.isReady) {
          this.logistics.set(status);
          this.canSubmit.set(this.form.valid);
        }
      },
    });
  }
}
