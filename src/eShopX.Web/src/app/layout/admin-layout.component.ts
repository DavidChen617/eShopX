import { CommonModule } from '@angular/common';
import { Component, computed, inject, OnDestroy, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../services/auth.service';

const SIDEBAR_WIDTH_KEY = 'eshopx.admin.sidebarWidth';
const DEFAULT_WIDTH = 256;
const COLLAPSED_WIDTH = 96;
const MIN_WIDTH = 220;
const MAX_WIDTH = 360;

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <div class="flex min-h-screen bg-slate-50">
      <aside
        class="sticky top-0 flex h-screen shrink-0 flex-col bg-slate-900 text-slate-300"
        [style.width.px]="sidebarWidth()"
      >
        <div class="flex items-center justify-between border-b border-slate-800 p-6">
          <div class="flex items-center gap-3 overflow-hidden">
            <div class="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg bg-indigo-500 text-white">
              <i class="pi pi-shield text-lg"></i>
            </div>
            @if (!isCollapsed()) {
              <span class="truncate text-xl font-black tracking-tighter text-white">eShopX Admin</span>
            }
          </div>

          <button
            type="button"
            (click)="toggleSidebarWidth()"
            class="flex h-9 w-9 shrink-0 items-center justify-center rounded-xl text-slate-400 transition-colors hover:bg-slate-800 hover:text-white"
            [attr.aria-label]="isCollapsed() ? '展開側邊欄' : '收合側邊欄'"
          >
            <i class="pi" [class.pi-angle-right]="isCollapsed()" [class.pi-angle-left]="!isCollapsed()"></i>
          </button>
        </div>

        <nav class="mt-4 flex-1 space-y-2 p-4">
          <a
            routerLink="/admin/products"
            routerLinkActive="bg-indigo-600 text-white shadow-lg shadow-indigo-600/20"
            class="group flex items-center gap-3 rounded-xl px-4 py-3 transition-all hover:bg-slate-800 hover:text-white"
            [class.justify-center]="isCollapsed()"
          >
            <i class="pi pi-box"></i>
            @if (!isCollapsed()) {
              <span class="font-bold">商品管理</span>
            }
          </a>
          <a
            routerLink="/admin/orders"
            routerLinkActive="bg-indigo-600 text-white shadow-lg shadow-indigo-600/20"
            class="group flex items-center gap-3 rounded-xl px-4 py-3 transition-all hover:bg-slate-800 hover:text-white"
            [class.justify-center]="isCollapsed()"
          >
            <i class="pi pi-shopping-cart"></i>
            @if (!isCollapsed()) {
              <span class="font-bold">訂單管理</span>
            }
          </a>
          <a
            routerLink="/admin/categories"
            routerLinkActive="bg-indigo-600 text-white shadow-lg shadow-indigo-600/20"
            class="group flex items-center gap-3 rounded-xl px-4 py-3 transition-all hover:bg-slate-800 hover:text-white"
            [class.justify-center]="isCollapsed()"
          >
            <i class="pi pi-list"></i>
            @if (!isCollapsed()) {
              <span class="font-bold">分類 / 尺寸管理</span>
            }
          </a>
        </nav>

        <div class="border-t border-slate-800 p-4">
          <a
            routerLink="/"
            class="flex items-center gap-3 rounded-xl px-4 py-3 transition-colors hover:bg-slate-800"
            [class.justify-center]="isCollapsed()"
          >
            <i class="pi pi-external-link"></i>
            @if (!isCollapsed()) {
              <span class="text-sm font-medium">返回商店首頁</span>
            }
          </a>
        </div>

        <button
          type="button"
          class="absolute right-0 top-0 h-full w-2 cursor-col-resize bg-transparent transition-colors hover:bg-indigo-500/30"
          (mousedown)="startResize($event)"
          aria-label="調整側邊欄寬度"
        ></button>
      </aside>

      <main class="flex-1 overflow-y-auto">
        <header class="sticky top-0 z-10 flex h-16 items-center justify-between border-b border-slate-200 bg-white px-8">
          <h2 class="text-sm font-bold uppercase tracking-widest text-slate-400">後台系統 / 控制台</h2>
          <div class="flex items-center gap-4">
            <div class="text-right">
              <p class="text-xs font-black text-slate-900">{{ authService.userName() || '管理員' }}</p>
              <p class="text-[10px] text-slate-400">{{ authService.userEmail() || '-' }}</p>
            </div>
            <div class="h-10 w-10 overflow-hidden rounded-full border-2 border-white bg-slate-200 shadow-sm">
              <img src="https://api.dicebear.com/7.x/avataaars/svg?seed=Admin" />
            </div>
          </div>
        </header>

        <div class="p-8">
          <router-outlet></router-outlet>
        </div>
      </main>
    </div>
  `,
})
export class AdminLayoutComponent implements OnDestroy {
  private removeMouseMove?: () => void;
  private removeMouseUp?: () => void;
  authService = inject(AuthService);

  sidebarWidth = signal(this.readSidebarWidth());
  isCollapsed = computed(() => this.sidebarWidth() <= COLLAPSED_WIDTH);

  toggleSidebarWidth() {
    const nextWidth = this.isCollapsed() ? DEFAULT_WIDTH : COLLAPSED_WIDTH;
    this.setSidebarWidth(nextWidth);
  }

  startResize(event: MouseEvent) {
    event.preventDefault();

    const onMouseMove = (moveEvent: MouseEvent) => {
      this.setSidebarWidth(moveEvent.clientX);
    };

    const onMouseUp = () => {
      this.removeResizeListeners();
    };

    window.addEventListener('mousemove', onMouseMove);
    window.addEventListener('mouseup', onMouseUp);

    this.removeMouseMove = () => window.removeEventListener('mousemove', onMouseMove);
    this.removeMouseUp = () => window.removeEventListener('mouseup', onMouseUp);
  }

  ngOnDestroy() {
    this.removeResizeListeners();
  }

  private setSidebarWidth(width: number) {
    const normalized = width < MIN_WIDTH
      ? COLLAPSED_WIDTH
      : Math.min(Math.max(width, MIN_WIDTH), MAX_WIDTH);

    this.sidebarWidth.set(normalized);

    if (typeof localStorage !== 'undefined') {
      localStorage.setItem(SIDEBAR_WIDTH_KEY, String(normalized));
    }
  }

  private readSidebarWidth(): number {
    if (typeof localStorage === 'undefined') {
      return DEFAULT_WIDTH;
    }

    const raw = Number(localStorage.getItem(SIDEBAR_WIDTH_KEY));
    if (!Number.isFinite(raw)) {
      return DEFAULT_WIDTH;
    }

    if (raw <= COLLAPSED_WIDTH) {
      return COLLAPSED_WIDTH;
    }

    return Math.min(Math.max(raw, MIN_WIDTH), MAX_WIDTH);
  }

  private removeResizeListeners() {
    this.removeMouseMove?.();
    this.removeMouseUp?.();
    this.removeMouseMove = undefined;
    this.removeMouseUp = undefined;
  }
}
