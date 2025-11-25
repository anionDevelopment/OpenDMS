import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EditDocumentDialogComponent } from './edit-document-dialog.component';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatDividerModule } from '@angular/material/divider';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatGridListModule } from '@angular/material/grid-list';
import { MatSelectModule } from '@angular/material/select';
import { MatMenuModule } from '@angular/material/menu';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatTreeModule } from '@angular/material/tree';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatInputModule } from '@angular/material/input';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatButtonModule } from '@angular/material/button';
import { MatTabsModule } from '@angular/material/tabs';
import { HomePageModule } from '../../home-page/home-page.module';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

describe('EditDocumentDialogComponent', () => {
  let component: EditDocumentDialogComponent;
  let fixture: ComponentFixture<EditDocumentDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        NoopAnimationsModule,
          CommonModule,
          ReactiveFormsModule,
          FormsModule,
          MatExpansionModule,
          MatFormFieldModule,
          MatDividerModule,
          MatCheckboxModule,
          MatGridListModule,
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
        EditDocumentDialogComponent,
      ],
      providers: [
        { provide: MAT_DIALOG_DATA, useValue: {
          documentDTO:{
            title:"documenttitle"
          }
        } },
        { provide: MatDialogRef, useValue: {} },
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(EditDocumentDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
