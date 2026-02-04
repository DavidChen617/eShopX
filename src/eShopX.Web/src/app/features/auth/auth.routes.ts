import { Routes } from '@angular/router';

import { LoginPageComponent } from './login.page';
import { GoogleCallbackPageComponent } from './google-callback.page';
import { LineCallbackPageComponent } from './line-callback.page';
import { RegisterPageComponent } from './register.page';

export const AUTH_ROUTES: Routes = [
  {
    path: 'login',
    component: LoginPageComponent,
  },
  {
    path: 'register',
    component: RegisterPageComponent,
  },
  {
    path: 'google/callback',
    component: GoogleCallbackPageComponent,
  },
  {
    path: 'line/callback',
    component: LineCallbackPageComponent,
  },
];
