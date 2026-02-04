import { Component, Input, OnChanges } from '@angular/core';


import { ProductImageDto } from '../../../core/models/api-models';

@Component({
  selector: 'ui-product-gallery',
  standalone: true,
  imports: [],
  templateUrl: './product-gallery.component.html',
})
export class ProductGalleryComponent implements OnChanges {
  @Input({ required: true }) images: ProductImageDto[] = [];
  @Input() alt = '';

  mainImageUrl: string | null = null;

  ngOnChanges(): void {
    this.mainImageUrl = this.images.find((image) => image.isPrimary)?.url ?? this.images[0]?.url ?? null;
  }

  setMain(url: string): void {
    this.mainImageUrl = url;
  }
}
