import { ComponentFixture, TestBed } from '@angular/core/testing';

import { HintNotFoundComponent } from './hint-not-found.component';

describe('HintNotFoundComponent', () => {
  let component: HintNotFoundComponent;
  let fixture: ComponentFixture<HintNotFoundComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HintNotFoundComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(HintNotFoundComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
