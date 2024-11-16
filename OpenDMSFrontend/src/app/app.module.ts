import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { AdminDashboardComponent } from './modules/admin/area/admin-dashboard/admin-dashboard.component';
import { AdminHomePageComponent } from './modules/admin/area/admin-home-page/admin-home-page.component';

@NgModule({
  declarations: [
    AppComponent,
    AdminDashboardComponent,
    AdminHomePageComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
