import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UserDashboardComponent } from './user-dashboard/user-dashboard.component';
import { UserSettingsComponent } from './user-settings/user-settings.component';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatButtonModule } from '@angular/material/button';
import { MatTabsModule } from '@angular/material/tabs';
import { UserAreaContainerComponent } from './user-area-container/user-area-container.component';
import { MatDividerModule } from '@angular/material/divider';
import { HomePageModule } from '../home-page/home-page.module';
import { MatIconModule } from '@angular/material/icon';
import { UserIconComponent } from './user-icon/user-icon.component';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatMenuModule } from '@angular/material/menu';
import { MatTableModule } from '@angular/material/table';
import { MatDialogModule } from '@angular/material/dialog';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSelectModule } from '@angular/material/select';
import { DocumentComponent } from './document/document.component';
import { DocumentsListComponent } from './documents-list/documents-list.component';

@NgModule({
  declarations: [
    DocumentComponent,
    DocumentsListComponent,
    UserIconComponent,
    UserAreaContainerComponent,
    UserDashboardComponent,
    UserSettingsComponent,
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    MatFormFieldModule,
    MatDividerModule,
    MatCheckboxModule,
    MatSelectModule,
    MatMenuModule,
    MatTooltipModule,
    MatInputModule,
    MatDialogModule,
    MatIconModule,
    MatTableModule,
    MatSidenavModule,
    MatButtonModule,
    MatTabsModule,
    HomePageModule,
  ]
})
export class UserAreaModule { }
