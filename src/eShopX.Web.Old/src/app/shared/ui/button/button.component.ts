import { Component, Input } from '@angular/core';
import { NgClass } from '@angular/common';

type ButtonVariant = 'primary' | 'secondary' | 'ghost';
type ButtonSize = 'sm' | 'md' | 'lg';

@Component({
  selector: 'ui-button',
  standalone: true,
  imports: [NgClass],
  templateUrl: './button.component.html',
})
export class ButtonComponent {
  @Input() variant: ButtonVariant = 'primary';
  @Input() size: ButtonSize = 'md';
  @Input() type: 'button' | 'submit' | 'reset' = 'button';
  @Input() disabled = false;

  get classes(): string {
    const base =
      'inline-flex items-center justify-center rounded-full font-medium transition focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 disabled:cursor-not-allowed disabled:opacity-60';

    const sizes: Record<ButtonSize, string> = {
      sm: 'h-9 px-4 text-sm',
      md: 'h-11 px-5 text-sm',
      lg: 'h-12 px-6 text-base',
    };

    const variants: Record<ButtonVariant, string> = {
      primary: 'bg-amber-400 text-neutral-900 hover:bg-amber-300 focus-visible:outline-amber-400',
      secondary:
        'border border-neutral-300 bg-white text-neutral-900 hover:border-neutral-400 hover:bg-neutral-50 focus-visible:outline-neutral-400',
      ghost: 'text-neutral-900 hover:bg-neutral-100 focus-visible:outline-neutral-300',
    };

    return [base, sizes[this.size], variants[this.variant]].join(' ');
  }
}
