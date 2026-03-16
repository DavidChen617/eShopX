import { CommonModule } from '@angular/common';
import { Component, computed, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { ToastModule } from 'primeng/toast';
import { CategoryService } from '../../services/category.service';
import { SizeService } from '../../services/size.service';

@Component({
  selector: 'app-category-size-management',
  standalone: true,
  imports: [CommonModule, FormsModule, TableModule, ButtonModule, InputTextModule, SelectModule, ToastModule],
  providers: [MessageService],
  template: `
    <div class="grid gap-6 xl:grid-cols-2">
      <p-toast position="top-right"></p-toast>

      <section class="overflow-hidden rounded-3xl border border-slate-200 bg-white shadow-sm">
        <div class="border-b border-slate-100 p-6">
          <h1 class="text-2xl font-black text-slate-900">分類管理</h1>
        </div>

        <div class="border-b border-slate-100 p-6">
          <div class="grid gap-3 md:grid-cols-[1fr,auto]">
            <input
              pInputText
              [(ngModel)]="newCategoryName"
              type="text"
              placeholder="新增分類名稱"
              class="w-full rounded-2xl bg-slate-100 px-4 py-2.5"
            />
            <button
              pButton
              type="button"
              label="新增"
              icon="pi pi-plus"
              (click)="addCategory()"
              class="rounded-xl border-none bg-indigo-600 px-4 py-2.5 text-white"
            ></button>
          </div>
        </div>

        <p-table [value]="categories()" styleClass="p-datatable-gridlines">
          <ng-template pTemplate="header">
            <tr class="bg-slate-50/50 text-xs uppercase tracking-widest text-slate-400">
              <th class="px-6 py-4">名稱</th>
              <th class="px-6 py-4">建立時間</th>
              <th class="px-6 py-4 text-right">操作</th>
            </tr>
          </ng-template>
          <ng-template pTemplate="body" let-category>
            <tr class="border-b border-slate-100 last:border-none">
              <td class="px-6 py-4">
                <input
                  pInputText
                  [(ngModel)]="category.name"
                  class="w-full rounded-xl"
                />
              </td>
              <td class="px-6 py-4 text-sm text-slate-600">{{ formatDate(category.createdAt) }}</td>
              <td class="px-6 py-4">
                <div class="flex justify-end gap-2">
                  <button
                    pButton
                    type="button"
                    icon="pi pi-save"
                    (click)="saveCategory(category.id, category.name)"
                    class="p-button-text rounded-xl text-slate-500 hover:text-indigo-600"
                  ></button>
                  <button
                    pButton
                    type="button"
                    icon="pi pi-trash"
                    (click)="removeCategory(category.id)"
                    class="p-button-text rounded-xl text-slate-500 hover:text-rose-500"
                  ></button>
                </div>
              </td>
            </tr>
          </ng-template>
        </p-table>
      </section>

      <section class="overflow-hidden rounded-3xl border border-slate-200 bg-white shadow-sm">
        <div class="border-b border-slate-100 p-6">
          <h2 class="text-2xl font-black text-slate-900">尺寸管理</h2>
        </div>

        <div class="grid gap-3 border-b border-slate-100 p-6 md:grid-cols-[1fr,180px,auto]">
          <input
            pInputText
            [(ngModel)]="newSizeName"
            type="text"
            placeholder="新增尺寸名稱"
            class="w-full rounded-2xl bg-slate-100 px-4 py-2.5"
          />
          <p-select
            [(ngModel)]="newSizeType"
            [options]="sizeTypeOptions"
            optionLabel="label"
            optionValue="value"
            placeholder="尺寸類型"
            styleClass="w-full"
          ></p-select>
          <button
            pButton
            type="button"
            label="新增"
            icon="pi pi-plus"
            (click)="addSize()"
            class="rounded-xl border-none bg-indigo-600 px-4 py-2.5 text-white"
          ></button>
        </div>

        <p-table [value]="sizes()" styleClass="p-datatable-gridlines">
          <ng-template pTemplate="header">
            <tr class="bg-slate-50/50 text-xs uppercase tracking-widest text-slate-400">
              <th class="px-6 py-4">名稱</th>
              <th class="px-6 py-4 text-center">類型</th>
              <th class="px-6 py-4">建立時間</th>
              <th class="px-6 py-4 text-right">操作</th>
            </tr>
          </ng-template>
          <ng-template pTemplate="body" let-size>
            <tr class="border-b border-slate-100 last:border-none">
              <td class="px-6 py-4">
                <input pInputText [(ngModel)]="size.name" class="w-full rounded-xl" />
              </td>
              <td class="px-6 py-4 text-center">
                <span class="text-sm font-medium text-slate-700">{{ size.type }}</span>
              </td>
              <td class="px-6 py-4 text-sm text-slate-600">{{ formatDate(size.createdAt) }}</td>
              <td class="px-6 py-4">
                <div class="flex justify-end gap-2">
                  <button
                    pButton
                    type="button"
                    icon="pi pi-save"
                    (click)="saveSize(size.id, size.name)"
                    class="p-button-text rounded-xl text-slate-500 hover:text-indigo-600"
                  ></button>
                  <button
                    pButton
                    type="button"
                    icon="pi pi-trash"
                    (click)="removeSize(size.id)"
                    class="p-button-text rounded-xl text-slate-500 hover:text-rose-500"
                  ></button>
                </div>
              </td>
            </tr>
          </ng-template>
        </p-table>
      </section>
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
    :host ::ng-deep .p-inputtext {
      @apply border-slate-200 focus:shadow-none;
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
  `],
})
export class CategorySizeManagementComponent {
  private categoryService = inject(CategoryService);
  private sizeService = inject(SizeService);
  private messageService = inject(MessageService);

  newCategoryName = '';
  newSizeName = '';
  newSizeType = 'Clothing';

  categories = computed(() => this.categoryService.categories());
  sizes = computed(() => this.sizeService.sizes());

  sizeTypeOptions = [
    { label: 'Clothing', value: 'Clothing' },
    { label: 'Pants', value: 'Pants' },
    { label: 'Shoes', value: 'Shoes' },
    { label: 'Hat', value: 'Hat' },
  ];

  addCategory() {
    const name = this.newCategoryName.trim();
    if (!name) return;
    this.categoryService.createCategory(name).subscribe({
      next: () => {
        this.newCategoryName = '';
        this.toast('分類已新增', name);
      },
      error: () => this.toastError('分類新增失敗'),
    });
  }

  saveCategory(id: string, name: string) {
    this.categoryService.renameCategory(id, name.trim()).subscribe({
      next: () => this.toast('分類已更新', name.trim()),
      error: () => this.toastError('分類更新失敗'),
    });
  }

  removeCategory(id: string) {
    this.categoryService.deleteCategory(id).subscribe({
      next: () => this.toast('分類已刪除', id),
      error: () => this.toastError('分類刪除失敗'),
    });
  }

  addSize() {
    const name = this.newSizeName.trim();
    if (!name) return;
    this.sizeService.createSize(name, this.newSizeType).subscribe({
      next: () => {
        this.newSizeName = '';
        this.toast('尺寸已新增', `${name} / ${this.newSizeType}`);
      },
      error: () => this.toastError('尺寸新增失敗'),
    });
  }

  saveSize(id: string, name: string) {
    this.sizeService.renameSize(id, name.trim()).subscribe({
      next: () => this.toast('尺寸已更新', name.trim()),
      error: () => this.toastError('尺寸更新失敗'),
    });
  }

  removeSize(id: string) {
    this.sizeService.deleteSize(id).subscribe({
      next: () => this.toast('尺寸已刪除', id),
      error: () => this.toastError('尺寸刪除失敗'),
    });
  }

  formatDate(value: string): string {
    return new Intl.DateTimeFormat('zh-TW', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
    }).format(new Date(value));
  }

  private toast(summary: string, detail: string) {
    this.messageService.add({
      severity: 'success',
      summary,
      detail,
      life: 2000,
    });
  }

  private toastError(summary: string) {
    this.messageService.add({
      severity: 'error',
      summary,
      detail: '請稍後再試，或確認後端 API 是否可用。',
      life: 2600,
    });
  }
}
