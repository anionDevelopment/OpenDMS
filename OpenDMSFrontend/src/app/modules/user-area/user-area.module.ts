import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UserDashboardComponent } from './user-dashboard/user-dashboard.component';
import { UserSettingsComponent } from './user-settings/user-settings.component';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatButtonModule } from '@angular/material/button';
import { MatPaginatedTabHeader, MatTabsModule } from '@angular/material/tabs';
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
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatTreeModule } from '@angular/material/tree';
import { DocumentTableComponent } from './document-table/document-table.component';
import { ContentTreeComponent } from './content-tree/content-tree.component';
import { MatAccordion, MatExpansionModule } from '@angular/material/expansion';
import { AddContentButtonComponent } from './add-content-button/add-content-button.component';
import { ContentViewComponent } from './content-view/content-view.component';

@NgModule({
  declarations: [
    AddContentButtonComponent,
    ContentTreeComponent,
    ContentViewComponent,
    DocumentComponent,
    DocumentTableComponent,
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
    MatExpansionModule,
    MatFormFieldModule,
    MatDividerModule,
    MatCheckboxModule,
    MatSelectModule,
    MatMenuModule,
    MatPaginatorModule,
    MatTreeModule,
    MatTooltipModule,
    MatInputModule,
    MatDialogModule,
    MatIconModule,
    MatTableModule,
    MatSidenavModule,
    MatButtonModule,
    MatTabsModule,
    HomePageModule
  ]
})
export class UserAreaModule { }
