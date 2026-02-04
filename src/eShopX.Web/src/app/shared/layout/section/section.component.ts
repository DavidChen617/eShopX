import { Component, Input } from '@angular/core';


import { ContainerComponent } from '../container/container.component';

@Component({
  selector: 'ui-section',
  standalone: true,
  imports: [ContainerComponent],
  templateUrl: './section.component.html',
})
export class SectionComponent {
  @Input() title?: string;
  @Input() subtitle?: string;
}
