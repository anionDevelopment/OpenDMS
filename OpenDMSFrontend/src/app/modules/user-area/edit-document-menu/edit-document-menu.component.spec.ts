import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EditDocumentMenuComponent } from './edit-document-menu.component';
import { MatDialogModule } from '@angular/material/dialog';
import { OpenDMSBackendService, UserService } from '../../../generated/open-dms-backend';
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
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatButtonModule } from '@angular/material/button';
import { MatTabsModule } from '@angular/material/tabs';
import { HomePageModule } from '../../home-page/home-page.module';

describe('EditDocumentMenuComponent', () => {
  let component: EditDocumentMenuComponent;
  let fixture: ComponentFixture<EditDocumentMenuComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
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
        HomePageModule,
      ],
      declarations: [
        EditDocumentMenuComponent,
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
          },
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(EditDocumentMenuComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
