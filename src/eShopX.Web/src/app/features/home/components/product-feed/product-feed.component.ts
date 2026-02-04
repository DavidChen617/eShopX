import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { NzIconModule } from 'ng-zorro-antd/icon';

import { RecommendProduct } from '../../../../core/models/home-models';

@Component({
  selector: 'app-product-feed',
  standalone: true,
  imports: [RouterLink, NzIconModule],
  templateUrl: './product-feed.component.html',
})
export class ProductFeedComponent {
  products = input.required<RecommendProduct[]>();
  title = input<string>('猜你喜歡');
}
