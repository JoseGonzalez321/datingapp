import { HttpEvent, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { BusyService } from '../services/busy-service';
import { delay, finalize, of, tap } from 'rxjs';

const cache = new Map<string, HttpEvent<unknown>>();

export const loadingInterceptor: HttpInterceptorFn = (req, next) => {
  const busyService = inject(BusyService);

  if (req.method === 'GET') {
    const cachedResponse = cache.get(req.url);
    if (cachedResponse) {
      // Return cached response immediately
      return of(cachedResponse);
    }
  }
  busyService.busy();
  return next(req).pipe(
    delay(500), // Simulate a delay for loading
    tap(response => {
      cache.set(req.url, response);
    }),
    finalize(() => {
      busyService.idle();
    })
  )
};
