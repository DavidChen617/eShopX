import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { catchError, map, throwError } from 'rxjs';

export interface ApiResponse<T> {
  result?: T | null;
  isError: boolean;
  statusCode: number;
  message?: string;
}

export const apiResponseInterceptor: HttpInterceptorFn = (req, next) =>
  next(req).pipe(
    map((event) => {
      if (!('body' in event) || event.body == null) {
        return event;
      }

      const body = event.body as ApiResponse<unknown>;
      if (typeof body !== 'object' || body.isError === undefined) {
        return event;
      }

      if (body.isError) {
        throw new Error(body.message || 'Request failed');
      }

      return event.clone({ body: body.result });
    }),
    catchError((err: unknown) => {
      if (err instanceof HttpErrorResponse && err.error && err.error.message) {
        return throwError(() => new Error(err.error.message));
      }
      return throwError(() => err);
    }),
  );
