import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminHomePageComponent } from './admin-home-page/admin-home-page.component';
import { AdminDashboardComponent } from './admin-dashboard/admin-dashboard.component';
import { UserComponent } from './user/user.component';
import { UsersListComponent } from './users-list/users-list.component';



@NgModule({
  declarations: [
    AdminHomePageComponent,
    AdminDashboardComponent,
    UserComponent,
    UsersListComponent,
  ],
  imports: [
    CommonModule
  ],
  exports: [
    AdminHomePageComponent,
  ]
})
export class AdminAreaModule { }
