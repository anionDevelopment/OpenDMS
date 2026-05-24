import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DocumentTableComponent } from './document-table.component';
import { StorageService } from '../../../services/storage.service';
import { OpenDMSBackendService } from '../../../generated/open-dms-backend';
import { Router } from '@angular/router';
import { UtilitiesService } from '../../../services/utilities.service';
import { CommonModule } from '@angular/common';
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
import { HomePageModule } from '../../home-page/home-page.module';
import { NoopAnimationDriver } from '@angular/animations/browser';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { MatSortModule } from '@angular/material/sort';
import { TimestampPipe } from '../../../pipes/timestamp.pipe';

describe('DocumentTableComponent', () => {
  let component: DocumentTableComponent;
  let fixture: ComponentFixture<DocumentTableComponent>;

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
        MatSortModule,
        HomePageModule
      ],
      declarations: [
        TimestampPipe,
        DocumentTableComponent,
      ],
      providers: [
        {
          provide: StorageService,
          useValue: {

          }
        },
        {
          provide: OpenDMSBackendService,
          useValue: {

          }
        },
        {
          provide: Router,
          useValue: {

          }
        },
        {
          provide: UtilitiesService,
          useValue: {

          }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(DocumentTableComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
