import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);

  if (!req.url.startsWith('/api/v1')) {
    return next(req);
  }

  const isPublicAuthRequest = [
    '/api/v1/auth/login',
    '/api/v1/auth/register',
    '/api/v1/auth/send-otp',
    '/api/v1/auth/refresh',
    '/api/v1/auth/google',
    '/api/v1/auth/line',
  ].includes(req.url);
  const isRefreshRequest = req.url === '/api/v1/auth/refresh';
  const canRefresh = !isPublicAuthRequest && !!authService.getRefreshToken();
  const expired = authService.isAccessTokenExpired();
  const withAuth = (token: string | null) =>
    token ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : req;

  if (canRefresh && expired) {
    return authService.refreshSession().pipe(
      switchMap((response) => next(withAuth(response.accessToken))),
      catchError((error) => {
        authService.clearSession();
        return throwError(() => error);
      })
    );
  }

  return next(isRefreshRequest ? req : withAuth(authService.getAccessToken())).pipe(
    catchError((error: unknown) => {
      if (!(error instanceof HttpErrorResponse) || error.status !== 401 || !canRefresh) {
        return throwError(() => error);
      }

      return authService.refreshSession().pipe(
        switchMap((response) => next(withAuth(response.accessToken))),
        catchError((refreshError: unknown) => {
          if (refreshError instanceof HttpErrorResponse && refreshError.url?.includes('/api/v1/auth/refresh')) {
            authService.clearSession();
          }

          return throwError(() => refreshError);
        })
      );
    })
  );
};
