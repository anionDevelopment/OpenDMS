import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FrameWorkComponent } from './frame-work.component';
import { CommonModule } from '@angular/common';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatCardModule } from '@angular/material/card';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { FooterComponent } from '../footer/footer.component';

describe('FrameWorkComponent', () => {
  let component: FrameWorkComponent;
  let fixture: ComponentFixture<FrameWorkComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({

      imports: [
        CommonModule,
        NoopAnimationsModule,
        ReactiveFormsModule,
        FormsModule,
        MatFormFieldModule,
        MatCardModule,
        MatInputModule,
        MatButtonModule,
        MatIconModule,
      ],
      declarations: [
        FooterComponent,
        FrameWorkComponent,
      ],
      providers: [
      ]

    }).compileComponents();

    fixture = TestBed.createComponent(FrameWorkComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
