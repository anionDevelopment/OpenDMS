import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EditDocumentMenuComponent } from './edit-document-menu.component';

describe('EditDocumentMenuComponent', () => {
  let component: EditDocumentMenuComponent;
  let fixture: ComponentFixture<EditDocumentMenuComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EditDocumentMenuComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EditDocumentMenuComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
