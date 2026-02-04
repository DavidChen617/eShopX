import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { NzCarouselModule } from 'ng-zorro-antd/carousel';

import { BannerSlide } from '../../../../core/models/home-models';

@Component({
  selector: 'app-banner-carousel',
  standalone: true,
  imports: [NzCarouselModule, RouterLink],
  templateUrl: './banner-carousel.component.html',
})
export class BannerCarouselComponent {
  slides = input.required<BannerSlide[]>();
}
