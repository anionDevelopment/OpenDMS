import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ViewDocumentComponent } from './view-document.component';
import { provideAnimations } from '@angular/platform-browser/animations';
import { OpenDMSBackendService, UserService } from '../../../generated/open-dms-backend';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatDividerModule } from '@angular/material/divider';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSelectModule } from '@angular/material/select';
import { HomePageModule } from '../../home-page/home-page.module';
import { MatTabsModule } from '@angular/material/tabs';
import { MatButtonModule } from '@angular/material/button';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatDialogModule } from '@angular/material/dialog';
import { MatInputModule } from '@angular/material/input';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatTreeModule } from '@angular/material/tree';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatMenuModule } from '@angular/material/menu';
import { RouterTestingModule } from '@angular/router/testing';
import { UserAreaContainerComponent } from '../user-area-container/user-area-container.component';
import { UserDataService } from '../../../services/user-data.service';
import { ActivatedRoute, Router } from '@angular/router';
import { of } from 'rxjs';
import { EditDocumentMenuComponent } from '../edit-document-menu/edit-document-menu.component';
import { DocumentPreviewComponent } from '../document-preview/document-preview.component';
import { UserIconComponent } from '../user-icon/user-icon.component';
import { TimestampPipe } from '../../../pipes/timestamp.pipe';

describe('ViewDocumentComponent', () => {
  let component: ViewDocumentComponent;
  let fixture: ComponentFixture<ViewDocumentComponent>;

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
        RouterTestingModule
      ],
      declarations: [
        TimestampPipe,
        UserIconComponent,
        EditDocumentMenuComponent,
        DocumentPreviewComponent,
        UserAreaContainerComponent,
        ViewDocumentComponent,
      ],
      providers: [
        provideAnimations(),
        {
          provide: OpenDMSBackendService,
          useValue: {
            aPIV3OpenDMSBackendGetDocumentFromReadableIdGet: () => of({
              id: 1,
            }),
          },
        },
        {
          provide: UserService,
          useValue: {
          },
        },
        {
          provide: UserDataService,
          useValue: {
            getAccessToken: () => "accesstoken1",
            userIsAdmin: () => of(true),
            getUserId: () => of('1'),
            getUserName: () => of('admin'),
          },
        },
        {
          provide: Router,
          useValue: {
            url: 'user/document/1',
            parseUrl: (url: string) => new Router().parseUrl(url),
          }
        },
        {
          provide: ActivatedRoute,
          useValue: {
            params: of({
              'readableId': '1'
            }),
          }
        },
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ViewDocumentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
