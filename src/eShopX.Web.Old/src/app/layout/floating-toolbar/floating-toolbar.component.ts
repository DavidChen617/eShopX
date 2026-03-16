import { Component, HostListener, signal } from '@angular/core';

import { Router } from '@angular/router';

@Component({
  selector: 'app-floating-toolbar',
  standalone: true,
  imports: [],
  templateUrl: './floating-toolbar.component.html',
})
export class FloatingToolbarComponent {
  isOpen = signal(false);
  query = signal('');

  constructor(private readonly router: Router) {}

  toggle(): void {
    this.isOpen.set(!this.isOpen());
  }

  close(): void {
    this.isOpen.set(false);
  }

  async submit(): Promise<void> {
    const q = this.query().trim();
    if (!q) return;
    await this.router.navigate(['/search'], { queryParams: { q } });
    this.close();
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    this.close();
  }
}
