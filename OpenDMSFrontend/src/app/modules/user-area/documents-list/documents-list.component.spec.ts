import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DocumentsListComponent } from './documents-list.component';
import { HomePageModule } from '../../home-page/home-page.module';

describe('DocumentsListComponent', () => {
  let component: DocumentsListComponent;
  let fixture: ComponentFixture<DocumentsListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        HomePageModule,
      ],
      declarations: [
        DocumentsListComponent,
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
