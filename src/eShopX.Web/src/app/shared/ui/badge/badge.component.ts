import { Component, Input } from '@angular/core';
import { NgClass } from '@angular/common';

type BadgeVariant = 'default' | 'accent' | 'muted';

@Component({
  selector: 'ui-badge',
  standalone: true,
  imports: [NgClass],
  templateUrl: './badge.component.html',
})
export class BadgeComponent {
  @Input() variant: BadgeVariant = 'default';

  get classes(): string {
    const base = 'inline-flex items-center rounded-full px-2.5 py-1 text-xs font-semibold';
    const variants: Record<BadgeVariant, string> = {
      default: 'bg-neutral-900 text-white',
      accent: 'bg-amber-200 text-amber-900',
      muted: 'bg-neutral-100 text-neutral-600',
    };

    return [base, variants[this.variant]].join(' ');
  }
}
