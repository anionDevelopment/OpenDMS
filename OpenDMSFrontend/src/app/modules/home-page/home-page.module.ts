import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HintNotAuthenticatedComponent } from './hint-not-authenticated/hint-not-authenticated.component';
import { HintNotAuthorizedComponent } from './hint-not-authorized/hint-not-authorized.component';
import { HintNotFoundComponent } from './hint-not-found/hint-not-found.component';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatIconModule } from '@angular/material/icon';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatInputModule } from '@angular/material/input';
import { LoginFormComponent } from './login-form/login-form.component';
import { HomePageComponent } from './home-page/home-page.component';
import { FooterComponent } from "./footer/footer.component";
import { FrameWorkComponent } from './frame-work/frame-work.component';



@NgModule({
  declarations: [
    FrameWorkComponent,
    FooterComponent,
    HintNotAuthenticatedComponent,
    HintNotAuthorizedComponent,
    HintNotFoundComponent,
    HomePageComponent,
    LoginFormComponent,
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    MatFormFieldModule,
    MatCardModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
  ],
  exports: [
    FooterComponent,
    FrameWorkComponent,
  ]
})
export class HomePageModule { }
