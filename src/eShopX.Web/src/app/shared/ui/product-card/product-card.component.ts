import { Component, Input } from '@angular/core';
import { RouterLink } from '@angular/router';

import { GetProductItems } from '../../../core/models/api-models';
import { BadgeComponent } from '../badge/badge.component';

@Component({
  selector: 'ui-product-card',
  standalone: true,
  imports: [RouterLink, BadgeComponent],
  templateUrl: './product-card.component.html',
})
export class ProductCardComponent {
  @Input({ required: true }) product!: GetProductItems;
  @Input() badge?: string;
  @Input() linkToDetail = true;
}
