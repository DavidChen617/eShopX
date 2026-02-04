import { Injectable } from '@angular/core';

import { IStorage } from './storage';

@Injectable({ providedIn: 'root' })
export class LocalStorageService implements IStorage {
  getItem(key: string): string | null {
    return window.localStorage.getItem(key);
  }

  setItem(key: string, value: string): void {
    window.localStorage.setItem(key, value);
  }

  removeItem(key: string): void {
    window.localStorage.removeItem(key);
  }
}
