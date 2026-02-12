import { Component, input } from '@angular/core';
import { Router } from '@angular/router';
import { NzCarouselModule } from 'ng-zorro-antd/carousel';

import { BannerSlide } from '../../../../core/models/home-models';

@Component({
  selector: 'app-banner-carousel',
  standalone: true,
  imports: [NzCarouselModule],
  templateUrl: './banner-carousel.component.html',
})
export class BannerCarouselComponent {
  slides = input.required<BannerSlide[]>();

  private startX = 0;
  private startY = 0;
  private mouseDown = false;
  private suppressClick = false;
  private suppressUntil = 0;
  private readonly dragThreshold = 10;

  constructor(private readonly router: Router) {}

  onMouseDown(event: MouseEvent): void {
    this.mouseDown = true;
    this.startX = event.clientX;
    this.startY = event.clientY;
    this.suppressClick = false;
  }

  onMouseMove(event: MouseEvent): void {
    if (!this.mouseDown) return;
    const deltaX = Math.abs(event.clientX - this.startX);
    const deltaY = Math.abs(event.clientY - this.startY);
    if (deltaX > this.dragThreshold || deltaY > this.dragThreshold) {
      this.suppressClick = true;
    }
  }

  onMouseUp(): void {
    this.mouseDown = false;
  }

  onTouchStart(event: TouchEvent): void {
    const touch = event.touches[0];
    if (!touch) return;
    this.startX = touch.clientX;
    this.startY = touch.clientY;
    this.suppressClick = false;
  }

  onTouchMove(event: TouchEvent): void {
    const touch = event.touches[0];
    if (!touch) return;
    const deltaX = Math.abs(touch.clientX - this.startX);
    const deltaY = Math.abs(touch.clientY - this.startY);
    if (deltaX > this.dragThreshold || deltaY > this.dragThreshold) {
      this.suppressClick = true;
    }
  }

  onSlideClick(event: MouseEvent, link: string): void {
    const now = Date.now();
    if (this.suppressClick || now < this.suppressUntil) {
      this.suppressUntil = now + 400;
      this.suppressClick = false;
      event.preventDefault();
      event.stopPropagation();
      return;
    }

    event.preventDefault();
    void this.router.navigateByUrl(link);
  }

  onTouchEnd(): void {
    if (this.suppressClick) {
      this.suppressUntil = Date.now() + 400;
    }
  }
}
