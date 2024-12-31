import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddContentButtonComponent } from './add-content-button.component';

describe('AddContentButtonComponent', () => {
  let component: AddContentButtonComponent;
  let fixture: ComponentFixture<AddContentButtonComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddContentButtonComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AddContentButtonComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
