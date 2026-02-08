import {
  ApplicationConfig,
  provideZonelessChangeDetection,
  importProvidersFrom,
} from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { apiBaseInterceptor } from './core/interceptors/api-base.interceptor';
import { apiResponseInterceptor } from './core/interceptors/api-response.interceptor';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { zh_TW, provideNzI18n } from 'ng-zorro-antd/i18n';
import { registerLocaleData } from '@angular/common';
import zh from '@angular/common/locales/zh-Hant';
import { NzIconModule } from 'ng-zorro-antd/icon';
import {
  HomeOutline,
  ShoppingCartOutline,
  UserOutline,
  SearchOutline,
  AppstoreOutline,
  FireOutline,
  ThunderboltOutline,
  RightOutline,
} from '@ant-design/icons-angular/icons';

import { routes } from './app.routes';

registerLocaleData(zh);

const icons = [
  HomeOutline,
  ShoppingCartOutline,
  UserOutline,
  SearchOutline,
  AppstoreOutline,
  FireOutline,
  ThunderboltOutline,
  RightOutline,
];

export const appConfig: ApplicationConfig = {
  providers: [
    provideZonelessChangeDetection(),
    provideRouter(routes),
    provideHttpClient(
      withInterceptors([apiBaseInterceptor, authInterceptor, apiResponseInterceptor]),
    ),
    provideAnimationsAsync(),
    provideNzI18n(zh_TW),
    importProvidersFrom(NzIconModule.forRoot(icons)),
  ],
};
