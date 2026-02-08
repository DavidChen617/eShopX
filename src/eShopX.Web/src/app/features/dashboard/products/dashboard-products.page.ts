import { Component, signal } from '@angular/core';

import { SellerProductService } from '../../../core/services/seller-product.service';
import { GetProductItems, ProductImageDto } from '../../../core/models/api-models';
import { HomepageService } from '../../../core/services/homepage.service';
import { CategoryEntry } from '../../../core/models/home-models';
import { SectionComponent } from '../../../shared/layout/section/section.component';
import { ButtonComponent } from '../../../shared/ui/button/button.component';

@Component({
  selector: 'app-dashboard-products-page',
  standalone: true,
  imports: [SectionComponent, ButtonComponent],
  templateUrl: './dashboard-products.page.html',
})
export class DashboardProductsPageComponent {
  items = signal<GetProductItems[]>([]);
  isLoading = signal(true);
  listError = signal<string | null>(null);
  createError = signal<string | null>(null);
  editError = signal<string | null>(null);
  saving = signal(false);

  editingId = signal<string | null>(null);
  isEditModalOpen = signal(false);
  createName = signal('');
  createDescription = signal('');
  createPrice = signal(0);
  createStockQuantity = signal(0);
  createIsActive = signal(true);

  editName = signal('');
  editDescription = signal('');
  editPrice = signal(0);
  editStockQuantity = signal(0);
  editIsActive = signal(true);
  createCategoryId = signal<string | null>(null);
  editCategoryId = signal<string | null>(null);

  categories = signal<CategoryEntry[]>([]);

  activeProductId = signal<string | null>(null);
  images = signal<Record<string, ProductImageDto[]>>({});
  imagesLoading = signal<Record<string, boolean>>({});
  imageSortDrafts = signal<Record<string, number>>({});
  uploadFile = signal<File | null>(null);
  uploadPrimary = signal(false);
  uploadSortOrder = signal(0);
  pendingImages = signal<File[]>([]);
  pendingPrimaryIndex = signal(0);

  constructor(
    private readonly sellerProductService: SellerProductService,
    private readonly homepageService: HomepageService,
  ) {
    void this.load();
  }

  async load(): Promise<void> {
    this.isLoading.set(true);
    this.listError.set(null);
    try {
      if (this.categories().length === 0) {
        const categories = await this.homepageService.getCategories();
        this.categories.set(categories);
      }
      const result = await this.sellerProductService.getMyProducts({
        page: 1,
        pageSize: 50,
      });
      this.items.set(result.items ?? []);
    } catch (err) {
      this.listError.set(err instanceof Error ? err.message : '載入失敗，請稍後再試。');
    } finally {
      this.isLoading.set(false);
    }
  }

  startCreate(): void {
    this.createName.set('');
    this.createDescription.set('');
    this.createPrice.set(0);
    this.createStockQuantity.set(0);
    this.createIsActive.set(true);
    this.createCategoryId.set(null);
    this.pendingImages.set([]);
    this.pendingPrimaryIndex.set(0);
    this.createError.set(null);
  }

  startEdit(item: GetProductItems): void {
    this.editingId.set(item.productId);
    this.editName.set(item.name);
    this.editDescription.set(item.description ?? '');
    this.editPrice.set(item.price);
    this.editStockQuantity.set(item.stockQuantity);
    this.editIsActive.set(item.isActive);
    this.editCategoryId.set(item.categoryId ?? null);
    this.activeProductId.set(item.productId);
    void this.loadImages(item.productId);
    this.editError.set(null);
    this.isEditModalOpen.set(true);
  }

  closeEditModal(): void {
    this.isEditModalOpen.set(false);
    this.editingId.set(null);
    this.editError.set(null);
    this.uploadFile.set(null);
    this.uploadPrimary.set(false);
    this.uploadSortOrder.set(0);
  }

  async saveCreate(): Promise<void> {
    if (this.saving()) return;
    if (!this.createName().trim()) {
      this.createError.set('請輸入商品名稱。');
      return;
    }

    this.saving.set(true);
    this.createError.set(null);
    try {
      const payload = {
        name: this.createName().trim(),
        description: this.createDescription().trim() || null,
        price: Number(this.createPrice()),
        stockQuantity: Number(this.createStockQuantity()),
        isActive: this.createIsActive(),
        categoryId: this.createCategoryId(),
      };

      const created = await this.sellerProductService.create(payload);
      const files = this.pendingImages();
      if (files.length > 0) {
        for (let i = 0; i < files.length; i += 1) {
          const isPrimary = i === this.pendingPrimaryIndex();
          await this.sellerProductService.uploadImage(created.productId, files[i], isPrimary, i);
        }
      }

      await this.load();
      this.startCreate();
    } catch (err) {
      this.createError.set(err instanceof Error ? err.message : '儲存失敗，請稍後再試。');
    } finally {
      this.saving.set(false);
    }
  }

  async saveEdit(): Promise<void> {
    if (this.saving()) return;
    if (!this.editingId()) return;
    if (!this.editName().trim()) {
      this.editError.set('請輸入商品名稱。');
      return;
    }

    this.saving.set(true);
    this.editError.set(null);
    try {
      const payload = {
        name: this.editName().trim(),
        description: this.editDescription().trim() || null,
        price: Number(this.editPrice()),
        stockQuantity: Number(this.editStockQuantity()),
        isActive: this.editIsActive(),
        categoryId: this.editCategoryId(),
      };

      await this.sellerProductService.update(this.editingId()!, payload);
      await this.load();
      this.closeEditModal();
    } catch (err) {
      this.editError.set(err instanceof Error ? err.message : '儲存失敗，請稍後再試。');
    } finally {
      this.saving.set(false);
    }
  }

  async remove(item: GetProductItems): Promise<void> {
    if (this.saving()) return;
    this.saving.set(true);
    this.listError.set(null);
    try {
      await this.sellerProductService.delete(item.productId);
      this.items.update((list) => list.filter((x) => x.productId !== item.productId));
    } catch (err) {
      this.listError.set(err instanceof Error ? err.message : '刪除失敗，請稍後再試。');
    } finally {
      this.saving.set(false);
    }
  }

  toggleImages(productId: string): void {
    if (this.activeProductId() === productId) {
      this.activeProductId.set(null);
      return;
    }
    this.activeProductId.set(productId);
    void this.loadImages(productId);
  }

  async loadImages(productId: string): Promise<void> {
    this.imagesLoading.update((map) => ({ ...map, [productId]: true }));
    try {
      const result = await this.sellerProductService.getImages(productId);
      this.images.update((map) => ({
        ...map,
        [productId]: result.images ?? [],
      }));
      this.imageSortDrafts.update((map) => {
        const next = { ...map };
        (result.images ?? []).forEach((img) => {
          next[img.id] = img.sortOrder;
        });
        return next;
      });
    } catch (err) {
      this.editError.set(err instanceof Error ? err.message : '載入圖片失敗。');
    } finally {
      this.imagesLoading.update((map) => ({ ...map, [productId]: false }));
    }
  }

  getImages(productId: string): ProductImageDto[] {
    return this.images()[productId] ?? [];
  }

  isImagesLoading(productId: string): boolean {
    return this.imagesLoading()[productId] ?? false;
  }

  getSortDraft(imageId: string): number {
    const draft = this.imageSortDrafts()[imageId];
    return typeof draft === 'number' ? draft : 0;
  }

  setSortDraft(imageId: string, value: number): void {
    this.imageSortDrafts.update((map) => ({ ...map, [imageId]: value }));
  }

  onFileChange(event: Event): void {
    const input = event.target as HTMLInputElement | null;
    const file = input?.files?.[0] ?? null;
    this.uploadFile.set(file);
  }

  getUploadFileName(): string {
    return this.uploadFile()?.name ?? '尚未選擇檔案';
  }

  onPendingImagesChange(event: Event): void {
    const input = event.target as HTMLInputElement | null;
    const files = input?.files ? Array.from(input.files) : [];
    if (files.length === 0) {
      return;
    }
    const combined = [...this.pendingImages(), ...files];
    this.pendingImages.set(combined);
    if (this.pendingPrimaryIndex() >= combined.length) {
      this.pendingPrimaryIndex.set(0);
    }
    if (input) {
      input.value = '';
    }
  }

  removePendingImage(index: number): void {
    const next = this.pendingImages().filter((_, i) => i !== index);
    this.pendingImages.set(next);
    if (this.pendingPrimaryIndex() >= next.length) {
      this.pendingPrimaryIndex.set(0);
    }
  }

  parseNumber(value: unknown, fallback = 0): number {
    const parsed = typeof value === 'string' ? Number(value) : Number(value);
    return Number.isFinite(parsed) ? parsed : fallback;
  }

  clearEditError(): void {
    if (this.editError()) {
      this.editError.set(null);
    }
  }

  clearCreateError(): void {
    if (this.createError()) {
      this.createError.set(null);
    }
  }

  async uploadImage(productId: string): Promise<void> {
    const file = this.uploadFile();
    if (!file) {
      this.editError.set('請選擇圖片檔案。');
      return;
    }
    this.saving.set(true);
    this.editError.set(null);
    try {
      await this.sellerProductService.uploadImage(
        productId,
        file,
        this.uploadPrimary(),
        this.uploadSortOrder(),
      );
      this.uploadFile.set(null);
      this.uploadPrimary.set(false);
      this.uploadSortOrder.set(0);
      await this.loadImages(productId);
    } catch (err) {
      this.editError.set(err instanceof Error ? err.message : '上傳失敗。');
    } finally {
      this.saving.set(false);
    }
  }

  async setPrimary(productId: string, imageId: string): Promise<void> {
    this.saving.set(true);
    this.editError.set(null);
    try {
      await this.sellerProductService.updateImage(productId, imageId, {
        isPrimary: true,
      });
      await this.loadImages(productId);
    } catch (err) {
      this.editError.set(err instanceof Error ? err.message : '更新主圖失敗。');
    } finally {
      this.saving.set(false);
    }
  }

  async updateSortOrder(productId: string, imageId: string, sortOrder: number): Promise<void> {
    this.saving.set(true);
    this.editError.set(null);
    try {
      await this.sellerProductService.updateImage(productId, imageId, {
        sortOrder,
      });
      await this.loadImages(productId);
    } catch (err) {
      this.editError.set(err instanceof Error ? err.message : '更新排序失敗。');
    } finally {
      this.saving.set(false);
    }
  }

  async removeImage(productId: string, imageId: string): Promise<void> {
    this.saving.set(true);
    this.editError.set(null);
    try {
      await this.sellerProductService.deleteImage(productId, imageId);
      await this.loadImages(productId);
    } catch (err) {
      this.editError.set(err instanceof Error ? err.message : '刪除圖片失敗。');
    } finally {
      this.saving.set(false);
    }
  }
}
