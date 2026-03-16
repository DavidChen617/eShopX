import { Component, input, signal, computed } from '@angular/core';
import { RouterLink } from '@angular/router';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzStatisticModule } from 'ng-zorro-antd/statistic';

import { FlashSaleData, FlashSaleSlot } from '../../../../core/models/flash-sale-models';
import { FlashSaleCardComponent } from '../flash-sale-card/flash-sale-card.component';

@Component({
  selector: 'app-flash-sale-section',
  standalone: true,
  imports: [RouterLink, NzIconModule, NzStatisticModule, FlashSaleCardComponent],
  templateUrl: './flash-sale-section.component.html',
})
export class FlashSaleSectionComponent {
  data = input.required<FlashSaleData>();

  selectedSlotIndex = signal(0);

  countdownDeadline = computed(() => {
    const d = this.data();
    return new Date(d.endsAt).getTime();
  });

  filteredItems = computed(() => {
    return this.data().items;
  });

  onSlotChange(index: number): void {
    this.selectedSlotIndex.set(index);
  }

  getSlotStatusText(slot: FlashSaleSlot): string {
    switch (slot.status) {
      case 'live':
        return '搶購中';
      case 'upcoming':
        return '即將開始';
      case 'ended':
        return '已結束';
      default:
        return '';
    }
  }

  isSlotSelected(index: number): boolean {
    return this.selectedSlotIndex() === index;
  }
}
