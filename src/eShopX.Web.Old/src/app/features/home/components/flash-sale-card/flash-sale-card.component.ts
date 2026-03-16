import { Component, input, computed } from '@angular/core';
import { NzProgressModule } from 'ng-zorro-antd/progress';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzTagModule } from 'ng-zorro-antd/tag';

import { FlashSaleItem } from '../../../../core/models/flash-sale-models';

@Component({
  selector: 'app-flash-sale-card',
  standalone: true,
  imports: [NzProgressModule, NzButtonModule, NzTagModule],
  templateUrl: './flash-sale-card.component.html',
})
export class FlashSaleCardComponent {
  item = input.required<FlashSaleItem>();

  soldPercent = computed(() => {
    const i = this.item();
    if (i.stockTotal <= 0) return 0;
    return Math.round(((i.stockTotal - i.stockRemaining) / i.stockTotal) * 100);
  });

  isSoldOut = computed(() => this.item().stockRemaining === 0);
}
