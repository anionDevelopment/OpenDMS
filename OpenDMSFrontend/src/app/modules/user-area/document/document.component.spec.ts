import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DocumentComponent } from './document.component';
import { HomePageModule } from '../../home-page/home-page.module';

describe('DocumentComponent', () => {
  let component: DocumentComponent;
  let fixture: ComponentFixture<DocumentComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        HomePageModule,
      ],
      declarations: [
        DocumentComponent,
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(DocumentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
