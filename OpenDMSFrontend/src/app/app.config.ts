import { ApplicationConfig, importProvidersFrom, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { BrowserAnimationsModule, provideAnimations } from '@angular/platform-browser/animations';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { BASE_PATH } from './generated/con-surv-backend/variables';
import { environment } from '../environments/environment';
import { logInterceptor } from './interceptors/log.interceptor';
import { Settings } from './static/Settings';

export const appConfig: ApplicationConfig = {

  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideAnimations(),
    importProvidersFrom([
      BrowserAnimationsModule,
      FormsModule,
      ReactiveFormsModule,
      CommonModule,
    ]),
    provideHttpClient(
      withInterceptors([
        logInterceptor,
      ]),
    ),
    {
      provide: BASE_PATH,
      useValue: Settings.getAPIUrl()
    },
  ]
};
