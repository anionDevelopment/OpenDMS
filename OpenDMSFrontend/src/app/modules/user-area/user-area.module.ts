import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UserDashboardComponent } from './user-dashboard/user-dashboard.component';
import { UserHomePageComponent } from './user-home-page/user-home-page.component';



@NgModule({
  declarations: [
    UserDashboardComponent,
    UserHomePageComponent
  ],
  imports: [
    CommonModule
  ],
  exports: [
    UserHomePageComponent,
  ]
})
export class UserAreaModule { }
