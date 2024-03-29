import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
} from '@angular/common/http';
import { delay, finalize, Observable } from 'rxjs';
import { BusyService } from '../_services/busy.service';

@Injectable()
export class BusyInterceptor implements HttpInterceptor {
  constructor(private busyService: BusyService) {}

  intercept(
    request: HttpRequest<unknown>,
    next: HttpHandler
  ): Observable<HttpEvent<unknown>> {
    if (request.url === 'https://localhost:5001/api/messages')
      return next.handle(request);
    this.busyService.busy();
    return next.handle(request).pipe(
      // delay(500), // production'da bu satırı yoruma al. Prod. buildinde fake delay istemeyiz.
      finalize(() => {
        this.busyService.idle();
      })
    );
  }
}
