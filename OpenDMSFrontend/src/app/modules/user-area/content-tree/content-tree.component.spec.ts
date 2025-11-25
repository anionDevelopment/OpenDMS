import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ContentTreeComponent } from './content-tree.component';
import { OpenDMSBackendService } from '../../../generated/open-dms-backend';
import { StorageService } from '../../../services/storage.service';
import { CommonModule } from '@angular/common';
import { HomePageModule } from '../../home-page/home-page.module';
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
import { of } from 'rxjs';
import { EditStorageLocationMenuComponent } from '../edit-storage-location-menu/edit-storage-location-menu.component';

describe('ContentTreeComponent', () => {
  let component: ContentTreeComponent;
  let fixture: ComponentFixture<ContentTreeComponent>;

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
        ContentTreeComponent,
        EditStorageLocationMenuComponent,
      ],
      providers: [
        {
          provide: StorageService,
          useValue: {
            getAccessToken: () => "accesstoken1",
          },
        },
        {
          provide: OpenDMSBackendService,
          useValue: {
            aPIV2OpenDMSBackendGetAllViewableStorageLocationsGet: () => of([]),
          },
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ContentTreeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
