import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FrameWorkComponent } from './frame-work.component';

describe('FrameWorkComponent', () => {
  let component: FrameWorkComponent;
  let fixture: ComponentFixture<FrameWorkComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [FrameWorkComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(FrameWorkComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
