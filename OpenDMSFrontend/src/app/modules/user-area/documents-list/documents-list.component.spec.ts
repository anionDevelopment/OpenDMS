import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DocumentsListComponent } from './documents-list.component';
import { HomePageModule } from '../../home-page/home-page.module';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { StorageService } from '../../../services/storage.service';
import { OpenDMSBackendService, UserService } from '../../../generated/open-dms-backend';
import { of } from 'rxjs';
import { UserAreaContainerComponent } from '../user-area-container/user-area-container.component';
import { UserDataService } from '../../../services/user-data.service';
import { Router } from '@angular/router';
import { Component } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatDividerModule } from '@angular/material/divider';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSelectModule } from '@angular/material/select';
import { MatMenuModule } from '@angular/material/menu';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatTreeModule } from '@angular/material/tree';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatInputModule } from '@angular/material/input';
import { MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatButtonModule } from '@angular/material/button';
import { MatTabsModule } from '@angular/material/tabs';
import { DocumentTableComponent } from '../document-table/document-table.component';
import { ContentTreeComponent } from '../content-tree/content-tree.component';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { SearchComponent } from '../search/search.component';
import { EditStorageLocationMenuComponent } from '../edit-storage-location-menu/edit-storage-location-menu.component';

@Component({
  selector: 'app-user-area-container',
  standalone: false,
  template: '<ng-content></ng-content>'
})
class UserAreaContainerComponentMock { }

describe('DocumentsListComponent', () => {
  let component: DocumentsListComponent;
  let fixture: ComponentFixture<DocumentsListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        CommonModule,
        NoopAnimationsModule,
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
      ],
      declarations: [
        ContentTreeComponent,
        DocumentTableComponent,
        EditStorageLocationMenuComponent,
        UserAreaContainerComponentMock,
        DocumentsListComponent,
        SearchComponent,
      ],
      providers: [
        {
          provide: UserService,
          useValue: {
          },
        },
        {
          provide: OpenDMSBackendService,
          useValue: {
            aPIV3OpenDMSBackendGetLatestDocumentsGet: () => of([]),
            aPIV3OpenDMSBackendGetAllViewableStorageLocationsGet: () => of([]),
          }
        },
        {
          provide: StorageService,
          useValue: {
            getAccessToken: () => "accesstoken1",
          }
        },
        {
          provide: UserDataService,
          useValue: {
          }
        },
        {
          provide: Router,
          useValue: {
            url: 'user/documents'
          }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(DocumentsListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
