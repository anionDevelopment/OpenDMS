import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HomePageComponent } from './home-page/home-page.component';
import { LoginFieldComponent } from './login-field/login-field.component';
import { ThemeSwitchComponent } from './theme-switch/theme-switch.component';



@NgModule({
  declarations: [
    HomePageComponent,
    LoginFieldComponent,
    ThemeSwitchComponent
  ],
  imports: [
    CommonModule
  ], exports: [
    HomePageComponent,
  ]
})
export class HomePageModule { }
