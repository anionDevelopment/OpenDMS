import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EditDocumentMenuComponent } from './edit-document-menu.component';
import { MatDialogModule } from '@angular/material/dialog';
import { UserService } from '../../../generated/open-dms-backend';

describe('EditDocumentMenuComponent', () => {
  let component: EditDocumentMenuComponent;
  let fixture: ComponentFixture<EditDocumentMenuComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        MatDialogModule,
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
