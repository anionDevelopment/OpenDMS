import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminHomePageComponent } from './admin-home-page/admin-home-page.component';
import { AdminDashboardComponent } from './admin-dashboard/admin-dashboard.component';
import { EditUserComponent } from './edit-user/edit-user.component';



@NgModule({
  declarations: [
    AdminHomePageComponent,
    AdminDashboardComponent,
    EditUserComponent
  ],
  imports: [
    CommonModule
  ]
})
export class AdminAreaModule { }
