import { ComponentFixture, TestBed } from '@angular/core/testing';
import { LinkToDocumentComponent } from './link-to-document.component';
import { UserService } from '../../../generated/open-dms-backend';

describe('LinkToDocumentComponent', () => {
  let component: LinkToDocumentComponent;
  let fixture: ComponentFixture<LinkToDocumentComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
      ],
      declarations: [
        LinkToDocumentComponent,
      ],
      providers: [
        {
          provide: UserService,
          useValue: {
          },
        },
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(LinkToDocumentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
