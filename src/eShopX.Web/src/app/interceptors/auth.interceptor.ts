import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { API_V1_BASE_URL } from '../shared/api.config';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);

  if (!req.url.startsWith(API_V1_BASE_URL)) {
    return next(req);
  }

  const isPublicAuthRequest = [
    `${API_V1_BASE_URL}/auth/login`,
    `${API_V1_BASE_URL}/auth/register`,
    `${API_V1_BASE_URL}/auth/send-otp`,
    `${API_V1_BASE_URL}/auth/refresh`,
    `${API_V1_BASE_URL}/auth/google`,
    `${API_V1_BASE_URL}/auth/line`,
  ].includes(req.url);
  const isRefreshRequest = req.url === `${API_V1_BASE_URL}/auth/refresh`;
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
          if (refreshError instanceof HttpErrorResponse && refreshError.url?.includes(`${API_V1_BASE_URL}/auth/refresh`)) {
            authService.clearSession();
          }

          return throwError(() => refreshError);
        })
      );
    })
  );
};
