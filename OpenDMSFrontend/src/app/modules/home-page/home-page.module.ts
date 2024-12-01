import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HomePageComponent } from './home-page/home-page.component';
import { LoginFieldComponent } from './login-field/login-field.component';
import { ThemeSwitchComponent } from './theme-switch/theme-switch.component';
import { FooterComponent } from './footer/footer.component';
import { FrameWorkComponent } from './frame-work/frame-work.component';



@NgModule({
  declarations: [
    HomePageComponent,
    LoginFieldComponent,
    ThemeSwitchComponent,
    FooterComponent,
    FrameWorkComponent
  ],
  imports: [
    CommonModule
  ], exports: [
    HomePageComponent,
  ]
})
export class HomePageModule { }
