import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UserAreaContainerComponent } from './user-area-container.component';

describe('UserAreaContainerComponent', () => {
  let component: UserAreaContainerComponent;
  let fixture: ComponentFixture<UserAreaContainerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [UserAreaContainerComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(UserAreaContainerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
