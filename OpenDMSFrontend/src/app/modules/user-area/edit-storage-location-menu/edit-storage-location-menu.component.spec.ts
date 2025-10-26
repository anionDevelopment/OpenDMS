import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EditStorageLocationMenuComponent } from './edit-storage-location-menu.component';

describe('EditStorageLocationMenuComponent', () => {
  let component: EditStorageLocationMenuComponent;
  let fixture: ComponentFixture<EditStorageLocationMenuComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EditStorageLocationMenuComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EditStorageLocationMenuComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
