import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { MultiSelectModule } from 'primeng/multiselect';
import { SelectModule } from 'primeng/select';
import { SelectButtonModule } from 'primeng/selectbutton';
import { StepperModule } from 'primeng/stepper';
import { TableModule } from 'primeng/table';
import { TextareaModule } from 'primeng/textarea';
import { map, Observable, of, switchMap } from 'rxjs';
import { AdminPrimaryButtonComponent } from '../../components/admin-primary-button.component';
import { Audience, Product, ProductUpsertPayload } from '../../models/api.models';
import { AdminProductService } from '../../services/admin-product.service';
import { CategoryService } from '../../services/category.service';
import { SizeService } from '../../services/size.service';
import { TagService } from '../../services/tag.service';

interface ProductFormSku {
  id?: string;
  sizeId: string;
  price: number;
  stock: number;
}

interface ProductFormImage {
  url: string;
  publicId: string;
  isPrimary: boolean;
  sortOrder: number;
  localObjectUrl?: boolean;
  file?: File;
}

interface ProductFormVariant {
  id?: string;
  color: string;
  skus: ProductFormSku[];
  images: ProductFormImage[];
}

interface ProductFormState {
  name: string;
  description: string;
  audience: Audience;
  categoryId: string;
  tagIds: string[];
  variants: ProductFormVariant[];
}

@Component({
  selector: 'app-product-form',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    ButtonModule,
    StepperModule,
    InputTextModule,
    TextareaModule,
    SelectModule,
    MultiSelectModule,
    SelectButtonModule,
    TableModule,
    AdminPrimaryButtonComponent,
  ],
  template: `
    <div class="mx-auto max-w-6xl">
      <div class="mb-8 flex items-center justify-between gap-4">
        <div class="flex items-center gap-4">
          <button
            pButton
            icon="pi pi-arrow-left"
            routerLink="/admin/products"
            class="p-button-rounded p-button-text text-slate-600"
          ></button>
          <div>
            <h1 class="text-2xl font-black text-slate-900">{{ isEditMode() ? '編輯商品' : '新增商品' }}</h1>
            <p class="text-sm text-slate-500">
              {{ isEditMode() ? '更新商品主檔、顏色變體與庫存設定' : '建立服飾商品，設定顏色、尺寸與圖片資料' }}
            </p>
          </div>
        </div>

        <div class="flex items-center gap-3">
          <app-admin-primary-button
            label="儲存商品"
            icon="pi pi-save"
            (pressed)="saveProduct()"
          />
        </div>
      </div>

      @if (errorMessage()) {
        <div class="mb-6 rounded-2xl border border-rose-200 bg-rose-50 px-5 py-4 text-sm font-medium text-rose-700">
          {{ errorMessage() }}
        </div>
      }

      @if (successMessage()) {
        <div class="mb-6 rounded-2xl border border-emerald-200 bg-emerald-50 px-5 py-4 text-sm font-medium text-emerald-700">
          {{ successMessage() }}
        </div>
      }

      <div class="rounded-3xl border border-slate-200 bg-white p-8 shadow-sm">
        <p-stepper [linear]="true" [value]="activeStep()">
          <p-step-list>
            <p-step [value]="1">基本資料</p-step>
            <p-step [value]="2">SKU</p-step>
            <p-step [value]="3">圖片與儲存</p-step>
          </p-step-list>

          <p-step-panels>
            <p-step-panel [value]="1">
              <ng-template #content>
                <div class="space-y-8 pt-6">
                  <div class="grid gap-6 md:grid-cols-2">
                    <div class="flex flex-col gap-2">
                      <label class="text-sm font-bold text-slate-700">商品名稱</label>
                      <input pInputText [(ngModel)]="formState.name" placeholder="例如：輕量防風外套" class="w-full rounded-xl" />
                    </div>

                    <div class="flex flex-col gap-2">
                      <label class="text-sm font-bold text-slate-700">商品分類</label>
                      <p-select
                        [(ngModel)]="formState.categoryId"
                        [options]="categoryOptions()"
                        optionLabel="name"
                        optionValue="id"
                        placeholder="選擇分類"
                        styleClass="w-full rounded-xl"
                      ></p-select>
                    </div>
                  </div>

                  <div class="flex flex-col gap-2">
                    <label class="text-sm font-bold text-slate-700">商品描述</label>
                    <textarea
                      pInputTextarea
                      [(ngModel)]="formState.description"
                      [autoResize]="true"
                      rows="7"
                      placeholder="請輸入商品的材質、版型與穿著情境"
                      class="min-h-40 w-full rounded-xl leading-7"
                    ></textarea>
                  </div>

                  <div class="grid gap-6 md:grid-cols-2">
                    <div class="flex flex-col gap-2">
                      <label class="text-sm font-bold text-slate-700">受眾</label>
                      <p-selectButton
                        [(ngModel)]="formState.audience"
                        [options]="audienceOptions"
                        optionLabel="label"
                        optionValue="value"
                        styleClass="w-fit"
                      ></p-selectButton>
                    </div>

                    <div class="flex flex-col gap-2">
                      <label class="text-sm font-bold text-slate-700">商品標籤</label>
                      <p-multiSelect
                        [(ngModel)]="formState.tagIds"
                        [options]="tagOptions()"
                        optionLabel="name"
                        optionValue="id"
                        placeholder="選擇標籤"
                        display="chip"
                        [maxSelectedLabels]="99"
                        styleClass="w-full rounded-xl"
                      ></p-multiSelect>
                    </div>

                  </div>

                  <div class="rounded-2xl border border-amber-200 bg-amber-50 px-5 py-4 text-sm text-amber-900">
                    商品建立/更新不直接處理上下架。
                  </div>

                  <div class="flex justify-end pt-4">
                    <app-admin-primary-button
                      label="下一步：設定顏色與尺寸"
                      icon="pi pi-arrow-right"
                      iconPos="right"
                      (pressed)="goToStep(2)"
                    />
                  </div>
                </div>
              </ng-template>
            </p-step-panel>

            <p-step-panel [value]="2">
              <ng-template #content>
                <div class="space-y-6 pt-6">
                  <div class="flex items-center justify-between">
                    <div>
                      <h2 class="text-lg font-black text-slate-900">顏色變體與尺寸庫存</h2>
                      <p class="text-sm text-slate-500">每個 Variant 對應一個顏色，底下維護尺寸 SKU 與價格庫存。</p>
                    </div>
                    <button
                      pButton
                      label="新增顏色 Variant"
                      icon="pi pi-plus"
                      (click)="addVariant()"
                      class="p-button-rounded p-button-outlined border-slate-300 text-slate-700"
                    ></button>
                  </div>

                  @for (variant of formState.variants; track variant; let variantIndex = $index) {
                    <section class="rounded-3xl border border-slate-200 bg-slate-50/70 p-6">
                      <div class="mb-5 flex items-start justify-between gap-4">
                        <div class="flex-1">
                          <label class="mb-2 block text-sm font-bold text-slate-700">顏色名稱</label>
                          <input
                            pInputText
                            [(ngModel)]="variant.color"
                            placeholder="例如：黑色、米白、深藍"
                            class="w-full rounded-xl bg-white"
                          />
                        </div>

                        <button
                          pButton
                          icon="pi pi-trash"
                          [disabled]="formState.variants.length === 1"
                          (click)="removeVariant(variantIndex)"
                          class="p-button-rounded p-button-text mt-7 text-rose-500"
                        ></button>
                      </div>

                      <div class="overflow-hidden rounded-2xl border border-slate-200 bg-white">
                        <p-table [value]="variant.skus" styleClass="p-datatable-sm">
                          <ng-template pTemplate="header">
                            <tr class="bg-slate-50 text-[11px] uppercase tracking-widest text-slate-400">
                              <th class="w-56 px-4 py-3">尺寸</th>
                              <th class="px-4 py-3">售價</th>
                              <th class="px-4 py-3">庫存</th>
                              <th class="px-4 py-3 text-right">操作</th>
                            </tr>
                          </ng-template>
                          <ng-template pTemplate="body" let-sku let-skuIndex="rowIndex">
                            <tr>
                              <td class="w-56 px-4 py-3">
                                <p-select
                                  [(ngModel)]="sku.sizeId"
                                  [options]="sizeOptions()"
                                  optionLabel="name"
                                  optionValue="id"
                                  placeholder="尺寸"
                                  [filter]="true"
                                  filterBy="name"
                                  appendTo="body"
                                  styleClass="w-full"
                                ></p-select>
                              </td>
                              <td class="px-4 py-3">
                                <input pInputText type="number" [(ngModel)]="sku.price" class="w-28 font-mono" />
                              </td>
                              <td class="px-4 py-3">
                                <input pInputText type="number" [(ngModel)]="sku.stock" class="w-24 font-mono" />
                              </td>
                              <td class="px-4 py-3 text-right">
                                <button
                                  pButton
                                  icon="pi pi-trash"
                                  [disabled]="variant.skus.length === 1"
                                  (click)="removeSku(variantIndex, skuIndex)"
                                  class="p-button-rounded p-button-text text-rose-500"
                                ></button>
                              </td>
                            </tr>
                          </ng-template>
                        </p-table>
                      </div>

                      <div class="mt-4 flex justify-between">
                        <span class="text-xs font-medium text-slate-500">
                          總庫存：{{ variantStock(variant) }} / 最低價格：TWD {{ minVariantPrice(variant) }}
                        </span>
                        <button
                          pButton
                          label="新增 SKU"
                          icon="pi pi-plus"
                          (click)="addSku(variantIndex)"
                          class="p-button-rounded p-button-text text-indigo-600"
                        ></button>
                      </div>
                    </section>
                  }

                  <div class="flex justify-between pt-4">
                    <button
                      pButton
                      label="返回基本資料"
                      icon="pi pi-arrow-left"
                      (click)="goToStep(1)"
                      class="p-button-rounded p-button-text text-slate-500"
                    ></button>
                    <app-admin-primary-button
                      label="下一步：設定圖片與檢查"
                      icon="pi pi-arrow-right"
                      iconPos="right"
                      (pressed)="goToStep(3)"
                    />
                  </div>
                </div>
              </ng-template>
            </p-step-panel>

            <p-step-panel [value]="3">
              <ng-template #content>
                <div class="space-y-8 pt-6">
                  <div class="grid gap-6 xl:grid-cols-[1.5fr,0.9fr]">
                    <div class="space-y-6">
                      @for (variant of formState.variants; track variant; let variantIndex = $index) {
                        <section class="rounded-3xl border border-slate-200 bg-slate-50/60 p-6">
                          <div class="mb-4 flex items-center justify-between">
                            <div>
                              <h3 class="text-lg font-black text-slate-900">{{ variant.color || '未命名顏色' }}</h3>
                              <p class="text-sm text-slate-500">這些圖片會綁在此顏色 Variant 底下。</p>
                            </div>
                            <label class="inline-flex cursor-pointer items-center gap-2 rounded-xl border border-slate-200 bg-white px-4 py-2.5 text-sm font-bold text-slate-700 transition-colors hover:border-slate-300 hover:bg-slate-50">
                              <i class="pi pi-upload"></i>
                              <span>上傳本地圖片</span>
                              <input
                                type="file"
                                accept="image/*"
                                multiple
                                class="hidden"
                                (change)="onLocalImagesSelected($event, variantIndex)"
                              />
                            </label>
                          </div>

                          <div class="space-y-4">
                            @for (image of variant.images; track image; let imageIndex = $index) {
                              <div class="grid gap-4 rounded-2xl border border-slate-200 bg-white p-4 md:grid-cols-[180px,1fr,100px,120px,auto]">
                                <div class="aspect-[4/5] overflow-hidden rounded-2xl bg-slate-100">
                                  @if (image.url) {
                                    <img
                                      [src]="image.url"
                                      [alt]="variant.color || '商品圖片'"
                                      class="h-full w-full object-cover"
                                    />
                                  } @else {
                                    <div class="flex h-full items-center justify-center text-sm font-medium text-slate-400">
                                      尚未上傳圖片
                                    </div>
                                  }
                                </div>

                                <div class="flex flex-col justify-between gap-3">
                                  <div>
                                    <label class="text-xs font-bold uppercase tracking-widest text-slate-400">檔案資訊</label>
                                    <div class="mt-2 break-all text-sm font-medium text-slate-700">{{ image.publicId }}</div>
                                  </div>

                                  <label class="inline-flex cursor-pointer items-center gap-2 rounded-xl border border-slate-200 bg-white px-3 py-2 text-sm font-bold text-slate-700 transition-colors hover:border-slate-300 hover:bg-slate-50">
                                    <i class="pi pi-refresh"></i>
                                    <span>替換圖片</span>
                                    <input
                                      type="file"
                                      accept="image/*"
                                      class="hidden"
                                      (change)="replaceLocalImage($event, variantIndex, imageIndex)"
                                    />
                                  </label>
                                </div>

                                <div class="flex flex-col gap-2">
                                  <label class="text-xs font-bold uppercase tracking-widest text-slate-400">排序</label>
                                  <input pInputText type="number" [(ngModel)]="image.sortOrder" class="w-full font-mono" />
                                </div>

                                <div class="flex flex-col gap-2">
                                  <label class="text-xs font-bold uppercase tracking-widest text-slate-400">主圖</label>
                                  <button
                                    pButton
                                    [label]="image.isPrimary ? '目前主圖' : '設為主圖'"
                                    [outlined]="!image.isPrimary"
                                    (click)="setPrimaryImage(variantIndex, imageIndex)"
                                    class="w-full p-button-sm p-button-rounded"
                                  ></button>
                                </div>

                                <div class="flex items-end justify-end">
                                  <button
                                    pButton
                                    icon="pi pi-trash"
                                    [disabled]="variant.images.length === 1"
                                    (click)="removeImage(variantIndex, imageIndex)"
                                    class="p-button-rounded p-button-text text-rose-500"
                                  ></button>
                                </div>
                              </div>
                            }
                          </div>
                        </section>
                      }
                    </div>

                    <aside class="h-fit rounded-3xl border border-slate-200 bg-slate-900 p-6 text-white">
                      <h3 class="text-lg font-black">送出前檢查</h3>
                      <div class="mt-6 space-y-4 text-sm">
                        <div class="rounded-2xl bg-white/5 p-4">
                          <div class="text-[11px] uppercase tracking-widest text-slate-400">商品</div>
                          <div class="mt-2 text-base font-bold">{{ formState.name || '未命名商品' }}</div>
                          <div class="mt-1 text-slate-300">{{ categoryLabel() }} / {{ audienceLabel() }}</div>
                        </div>

                        <div class="rounded-2xl bg-white/5 p-4">
                          <div class="text-[11px] uppercase tracking-widest text-slate-400">標籤</div>
                          <div class="mt-2 flex flex-wrap gap-2">
                            @for (tag of selectedTagNames(); track tag) {
                              <span class="rounded-full bg-white/10 px-3 py-1 text-xs font-bold">{{ tag }}</span>
                            }
                          </div>
                        </div>

                        <div class="rounded-2xl bg-white/5 p-4">
                          <div class="text-[11px] uppercase tracking-widest text-slate-400">變體摘要</div>
                          <div class="mt-3 space-y-3">
                            @for (variant of formState.variants; track variant) {
                              <div class="rounded-xl border border-white/10 px-3 py-3">
                                <div class="font-bold">{{ variant.color || '未命名顏色' }}</div>
                                <div class="mt-1 text-xs text-slate-300">
                                  {{ variant.skus.length }} 個 SKU / {{ variant.images.length }} 張圖 / 庫存 {{ variantStock(variant) }}
                                </div>
                              </div>
                            }
                          </div>
                        </div>

                        <div class="rounded-2xl bg-white/5 p-4 text-xs leading-6 text-slate-300">
                          目前使用本地圖片預覽。正式串接時，選取檔案後應先上傳到 POST /api/v1/admin/products/images，再以回傳的 url 與 publicId 送出商品資料。
                        </div>
                      </div>

                      <div class="mt-6 space-y-3">
                        <button
                          pButton
                          label="返回上一階段"
                          icon="pi pi-arrow-left"
                          (click)="goToStep(2)"
                          class="p-button-rounded p-button-outlined w-full border-white/20 text-white"
                        ></button>
                        <app-admin-primary-button
                          [label]="isEditMode() ? '更新商品' : '建立商品'"
                          icon="pi pi-check"
                          tone="emerald"
                          [fullWidth]="true"
                          (pressed)="saveProduct()"
                        />
                      </div>
                    </aside>
                  </div>
                </div>
              </ng-template>
            </p-step-panel>
          </p-step-panels>
        </p-stepper>
      </div>
    </div>
  `,
  styles: [
    `
      @reference "tailwindcss";

      :host ::ng-deep .p-step-header {
        @apply p-0;
      }

      :host ::ng-deep .p-step-number {
        @apply flex h-9 w-9 items-center justify-center rounded-full bg-slate-100 text-xs font-bold text-slate-500;
      }

      :host ::ng-deep .p-step.p-step-active .p-step-number {
        @apply bg-indigo-600 text-white shadow-lg shadow-indigo-600/20;
      }

      :host ::ng-deep .p-step-title {
        @apply ml-2 text-xs font-black uppercase tracking-widest text-slate-400;
      }

      :host ::ng-deep .p-step.p-step-active .p-step-title {
        @apply text-slate-900;
      }

      :host ::ng-deep .p-select,
      :host ::ng-deep .p-multiselect,
      :host ::ng-deep .p-inputtext,
      :host ::ng-deep .p-textarea {
        @apply border-slate-200;
      }

      :host ::ng-deep .p-selectbutton {
        @apply inline-flex gap-2;
      }

      :host ::ng-deep .p-selectbutton .p-togglebutton {
        @apply min-w-24 rounded-xl border border-slate-200 bg-white px-4 py-2.5 text-slate-600 shadow-none transition-all;
      }

      :host ::ng-deep .p-selectbutton .p-togglebutton .p-togglebutton-content {
        @apply px-0 py-0;
      }

      :host ::ng-deep .p-selectbutton .p-togglebutton .p-togglebutton-label {
        @apply text-sm font-bold tracking-wide;
      }

      :host ::ng-deep .p-selectbutton .p-togglebutton.p-togglebutton-checked {
        @apply border-slate-900 text-slate-900;
      }

      :host ::ng-deep .p-datatable .p-datatable-thead > tr > th {
        @apply border-none bg-slate-50;
      }

      :host ::ng-deep .p-datatable .p-datatable-tbody > tr > td {
        @apply border-none;
      }

    `,
  ],
})
export class ProductFormComponent {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private adminProductService = inject(AdminProductService);
  private categoryService = inject(CategoryService);
  private tagService = inject(TagService);
  private sizeService = inject(SizeService);

  private productId = this.route.snapshot.paramMap.get('id');

  activeStep = signal(1);
  errorMessage = signal('');
  successMessage = signal('');
  isSaving = signal(false);
  isLoading = signal(!!this.productId);
  formState: ProductFormState = this.createEmptyForm();

  isEditMode = computed(() => !!this.productId);
  categoryOptions = computed(() => this.categoryService.categories());
  tagOptions = computed(() => this.tagService.tags());
  sizeOptions = computed(() => this.sizeService.sizes());

  audienceOptions = [
    { label: '男裝', value: 'Men' as Audience },
    { label: '女裝', value: 'Women' as Audience },
  ];

  constructor() {
    if (this.productId) {
      this.adminProductService.getById(this.productId).subscribe({
        next: (product) => {
          this.formState = this.mapProductToForm(product);
          this.isLoading.set(false);
        },
        error: () => {
          this.errorMessage.set(`找不到商品 ${this.productId}`);
          this.isLoading.set(false);
        },
      });
    } else {
      this.isLoading.set(false);
    }
  }

  goToStep(step: number) {
    this.activeStep.set(step);
  }

  categoryLabel(): string {
    return this.categoryService.getCategoryName(this.formState.categoryId);
  }

  audienceLabel(): string {
    return this.formState.audience === 'Men' ? '男裝' : '女裝';
  }

  selectedTagNames(): string[] {
    return this.formState.tagIds.map((id) => this.tagService.getTagName(id));
  }

  addVariant() {
    this.formState.variants.push(this.createEmptyVariant());
  }

  removeVariant(variantIndex: number) {
    this.formState.variants.splice(variantIndex, 1);
  }

  addSku(variantIndex: number) {
    this.formState.variants[variantIndex].skus.push(this.createEmptySku());
  }

  removeSku(variantIndex: number, skuIndex: number) {
    this.formState.variants[variantIndex].skus.splice(skuIndex, 1);
  }

  onLocalImagesSelected(event: Event, variantIndex: number) {
    const input = event.target as HTMLInputElement;
    const files = Array.from(input.files ?? []);
    if (files.length === 0) return;

    const variant = this.formState.variants[variantIndex];
    const hasPrimary = variant.images.some((image) => !!image.url);

    const nextImages = files.map((file, index) => this.createLocalImage(file, !hasPrimary && index === 0, variant.images.length + index));
    variant.images = variant.images.filter((image) => image.url).concat(nextImages);
    input.value = '';
  }

  replaceLocalImage(event: Event, variantIndex: number, imageIndex: number) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    const variant = this.formState.variants[variantIndex];
    const currentImage = variant.images[imageIndex];
    if (currentImage.localObjectUrl && currentImage.url) {
      URL.revokeObjectURL(currentImage.url);
    }

    variant.images[imageIndex] = this.createLocalImage(file, currentImage.isPrimary, currentImage.sortOrder);
    input.value = '';
  }

  removeImage(variantIndex: number, imageIndex: number) {
    const variant = this.formState.variants[variantIndex];
    const removedImage = variant.images[imageIndex];
    if (removedImage?.localObjectUrl && removedImage.url) {
      URL.revokeObjectURL(removedImage.url);
    }
    variant.images.splice(imageIndex, 1);
    const hasPrimary = variant.images.some((image) => image.isPrimary);
    variant.images.forEach((image, index) => {
      image.sortOrder = index;
      if (!hasPrimary) {
        image.isPrimary = index === 0;
      }
    });
  }

  setPrimaryImage(variantIndex: number, imageIndex: number) {
    this.formState.variants[variantIndex].images.forEach((image, index) => {
      image.isPrimary = index === imageIndex;
    });
  }

  variantStock(variant: ProductFormVariant): number {
    return variant.skus.reduce((sum, sku) => sum + Number(sku.stock || 0), 0);
  }

  minVariantPrice(variant: ProductFormVariant): number {
    return variant.skus.reduce((min, sku) => Math.min(min, Number(sku.price || 0)), variant.skus[0]?.price ?? 0);
  }

  saveProduct() {
    this.errorMessage.set('');
    this.successMessage.set('');

    const validationError = this.validateForm();
    if (validationError) {
      this.errorMessage.set(validationError);
      return;
    }

    this.isSaving.set(true);

    this.uploadPendingImages().pipe(
      switchMap(() => {
        const payload = this.buildPayload();
        if (this.isEditMode() && this.productId) {
          return this.adminProductService.update(this.productId, payload).pipe(map(() => '商品已更新'));
        }

        return this.adminProductService.create(payload).pipe(map(() => '商品已建立'));
      })
    ).subscribe({
      next: (message) => {
        this.isSaving.set(false);
        this.successMessage.set(message);
        setTimeout(() => {
          void this.router.navigate(['/admin/products']);
        }, 300);
      },
      error: () => {
        this.isSaving.set(false);
        this.errorMessage.set('儲存商品失敗，請稍後再試，或確認後端 API 是否可用。');
      },
    });
  }

  private validateForm(): string {
    const form = this.formState;

    if (!form.name.trim()) return '請輸入商品名稱。';
    if (!form.categoryId) return '請選擇商品分類。';
    if (!form.description.trim()) return '請輸入商品描述。';
    if (form.variants.length === 0) return '至少需要一個顏色 Variant。';

    for (const [variantIndex, variant] of form.variants.entries()) {
      if (!variant.color.trim()) {
        return `第 ${variantIndex + 1} 個 Variant 尚未填寫顏色名稱。`;
      }

      if (variant.skus.length === 0) {
        return `第 ${variantIndex + 1} 個 Variant 至少需要一個 SKU。`;
      }

      if (variant.images.length === 0) {
        return `第 ${variantIndex + 1} 個 Variant 至少需要一張圖片。`;
      }

      if (!variant.images.some((image) => image.isPrimary)) {
        return `第 ${variantIndex + 1} 個 Variant 需要指定主圖。`;
      }

      for (const [skuIndex, sku] of variant.skus.entries()) {
        if (!sku.sizeId) {
          return `第 ${variantIndex + 1} 個 Variant 的第 ${skuIndex + 1} 個 SKU 尚未選擇尺寸。`;
        }

        if (sku.price < 0 || sku.stock < 0) {
          return `第 ${variantIndex + 1} 個 Variant 的 SKU 價格與庫存不可為負數。`;
        }
      }

      for (const [imageIndex, image] of variant.images.entries()) {
        if (!image.url.trim()) {
          return `第 ${variantIndex + 1} 個 Variant 的第 ${imageIndex + 1} 張圖片尚未填寫 URL。`;
        }

        if (!image.publicId.trim()) {
          return `第 ${variantIndex + 1} 個 Variant 的第 ${imageIndex + 1} 張圖片尚未填寫 publicId。`;
        }
      }
    }

    return '';
  }

  private buildPayload(): ProductUpsertPayload {
    const form = this.formState;

    return {
      name: form.name.trim(),
      description: form.description.trim(),
      audience: form.audience,
      categoryId: form.categoryId,
      tagIds: [...new Set(form.tagIds)],
      variants: form.variants.map((variant) => ({
        variantId: variant.id,
        color: variant.color.trim(),
        skus: variant.skus.map((sku) => ({
          skuId: sku.id,
          sizeId: sku.sizeId,
          price: Number(sku.price),
          stock: Number(sku.stock),
        })),
        images: variant.images.map((image, index) => ({
          url: image.url.trim(),
          publicId: image.publicId.trim(),
          isPrimary: image.isPrimary,
          sortOrder: Number.isFinite(image.sortOrder) ? Number(image.sortOrder) : index,
        })),
      })),
    };
  }

  private createEmptyForm(): ProductFormState {
    return {
      name: '',
      description: '',
      audience: 'Men',
      categoryId: '',
      tagIds: [],
      variants: [this.createEmptyVariant('黑色')],
    };
  }

  private createEmptyVariant(color = ''): ProductFormVariant {
    return {
      color,
      skus: this.sizeService.sizes().map((size) => ({
        sizeId: size.id,
        price: 0,
        stock: 0,
      })),
      images: [],
    };
  }

  private createEmptySku(): ProductFormSku {
    return {
      sizeId: this.sizeService.sizes()[0]?.id ?? '',
      price: 0,
      stock: 0,
    };
  }

  private createEmptyImage(isPrimary: boolean): ProductFormImage {
    return {
      url: '',
      publicId: '',
      isPrimary,
      sortOrder: 0,
    };
  }

  private createLocalImage(file: File, isPrimary: boolean, sortOrder: number): ProductFormImage {
    return {
      url: URL.createObjectURL(file),
      publicId: this.buildMockPublicId(file.name),
      isPrimary,
      sortOrder,
      localObjectUrl: true,
      file,
    };
  }

  private uploadPendingImages(): Observable<void> {
    const pendingImages = this.formState.variants
      .flatMap((variant) => variant.images)
      .filter((image) => !!image.file);

    if (pendingImages.length === 0) {
      return of(void 0);
    }

    return this.adminProductService.uploadImages(pendingImages.map((image) => image.file!)).pipe(
      map((uploadedImages) => {
        uploadedImages.forEach((uploadedImage, index) => {
          const currentImage = pendingImages[index];
          if (!currentImage) {
            return;
          }

          if (currentImage.localObjectUrl && currentImage.url) {
            URL.revokeObjectURL(currentImage.url);
          }

          currentImage.url = uploadedImage.url;
          currentImage.publicId = uploadedImage.publicId;
          currentImage.localObjectUrl = false;
          currentImage.file = undefined;
        });
      })
    );
  }

  private buildMockPublicId(fileName: string): string {
    const normalized = fileName
      .toLowerCase()
      .replace(/\.[^/.]+$/, '')
      .replace(/[^a-z0-9]+/g, '-')
      .replace(/^-+|-+$/g, '');

    return `mock-upload/${Date.now()}-${normalized || 'image'}`;
  }

  private mapProductToForm(product: Product): ProductFormState {
    return {
      name: product.name,
      description: product.description,
      audience: product.audience,
      categoryId: product.categoryId,
      tagIds: [...product.tagIds],
      variants: product.variants.map((variant) => ({
        id: variant.id,
        color: variant.color,
        skus: variant.skus.map((sku) => ({
          id: sku.id,
          sizeId: sku.sizeId,
          price: sku.price,
          stock: sku.stock,
        })),
        images: variant.images.map((image) => ({
          url: image.url,
          publicId: image.publicId,
          isPrimary: image.isPrimary,
          sortOrder: image.sortOrder,
          localObjectUrl: false,
        })),
      })),
    };
  }
}
