import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EditContainerDialogComponent } from './edit-container-dialog.component';

describe('EditContainerDialogComponent', () => {
  let component: EditContainerDialogComponent;
  let fixture: ComponentFixture<EditContainerDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EditContainerDialogComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EditContainerDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
