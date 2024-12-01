import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UserDashboardComponent } from './user-dashboard/user-dashboard.component';
import { UserHomePageComponent } from './user-home-page/user-home-page.component';
import { UserAreaContainerComponent } from './user-area-container/user-area-container.component';
import { UserIconComponent } from './user-icon/user-icon.component';
import { UserSettingsComponent } from './user-settings/user-settings.component';
import { DocumentsListComponent } from './documents-list/documents-list.component';
import { DocumentComponent } from './document/document.component';



@NgModule({
  declarations: [
    UserDashboardComponent,
    UserHomePageComponent,
    UserAreaContainerComponent,
    UserIconComponent,
    UserSettingsComponent,
    DocumentsListComponent,
    DocumentComponent
  ],
  imports: [
    CommonModule
  ],
  exports: [
    UserHomePageComponent,
  ]
})
export class UserAreaModule { }
