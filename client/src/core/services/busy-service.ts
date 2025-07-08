import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class BusyService {
  busyRequestsCount = signal(0);

  busy() {
    this.busyRequestsCount.update(current => current + 1);
  }

  idle() {
    this.busyRequestsCount.update(current => Math.max(0, current - 1));
  }
}
