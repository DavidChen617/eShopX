import { HttpInterceptorFn } from '@angular/common/http';

const API_BASE = '';

export const apiBaseInterceptor: HttpInterceptorFn = (req, next) => {
  if (req.url.startsWith('http://') || req.url.startsWith('https://')) {
    return next(req);
  }

  const normalized = req.url.startsWith('/') ? req.url : `/${req.url}`;
  return next(req.clone({ url: `${API_BASE}${normalized}` }));
};
