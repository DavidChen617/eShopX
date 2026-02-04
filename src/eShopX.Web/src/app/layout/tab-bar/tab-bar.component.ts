import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzBadgeModule } from 'ng-zorro-antd/badge';

@Component({
  selector: 'app-tab-bar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, NzIconModule, NzBadgeModule],
  templateUrl: './tab-bar.component.html',
})
export class TabBarComponent {
  cartCount = 3; // Mock 購物車數量
}
