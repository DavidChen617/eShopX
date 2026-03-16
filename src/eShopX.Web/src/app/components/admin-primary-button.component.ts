import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';

@Component({
  selector: 'app-admin-primary-button',
  standalone: true,
  imports: [CommonModule, ButtonModule, RouterLink],
  template: `
    <button
      pButton
      type="button"
      [label]="label"
      [icon]="icon || ''"
      [iconPos]="iconPos"
      [routerLink]="routerLink"
      [class]="buttonClass"
      (click)="pressed.emit()"
    ></button>
  `,
})
export class AdminPrimaryButtonComponent {
  @Input({ required: true }) label!: string;
  @Input() icon?: string;
  @Input() iconPos: 'left' | 'right' | 'top' | 'bottom' = 'left';
  @Input() routerLink: string | any[] | null = null;
  @Input() fullWidth = false;
  @Input() tone: 'indigo' | 'emerald' = 'indigo';
  @Output() pressed = new EventEmitter<void>();

  get buttonClass(): string {
    const base = 'rounded-xl border-none px-6 py-3.5';
    const width = this.fullWidth ? 'w-full' : '';
    const tone = this.tone === 'emerald'
      ? 'bg-emerald-500 text-slate-950'
      : 'bg-indigo-600 text-white';

    return [base, width, tone].filter(Boolean).join(' ');
  }
}
