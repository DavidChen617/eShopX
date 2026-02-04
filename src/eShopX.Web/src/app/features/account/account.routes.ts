import { Routes } from '@angular/router';

import { AccountPageComponent } from './account.page';
import { AccountSettingsPageComponent } from './account-settings.page';

export const ACCOUNT_ROUTES: Routes = [
  {
    path: '',
    component: AccountPageComponent,
  },
  {
    path: 'settings',
    component: AccountSettingsPageComponent,
  },
];
