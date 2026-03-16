import { HttpInterceptorFn } from '@angular/common/http';

import { environment } from '../../../environments/environment';

const API_BASE = environment.apiBaseUrl || '';

export const apiBaseInterceptor: HttpInterceptorFn = (req, next) => {
  if (req.url.startsWith('http://') || req.url.startsWith('https://')) {
    return next(req);
  }

  let normalized = req.url.startsWith('/') ? req.url : `/${req.url}`;
  const base = API_BASE.replace(/\/$/, '');
  if (base.endsWith('/api') && normalized.startsWith('/api')) {
    normalized = normalized.replace(/^\/api/, '');
  }
  return next(req.clone({ url: `${base}${normalized}` }));
};
