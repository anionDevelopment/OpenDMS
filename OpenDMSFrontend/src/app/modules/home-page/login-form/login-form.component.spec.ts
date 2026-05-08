import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LoginFormComponent } from './login-form.component';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatCardModule } from '@angular/material/card';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { OIDCService, UserService } from '../../../generated/open-dms-backend';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { of } from 'rxjs';

describe('LoginFormComponent', () => {
  let component: LoginFormComponent;
  let fixture: ComponentFixture<LoginFormComponent>;

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
        MatSelectModule,
      ],
      declarations: [
        LoginFormComponent,
      ],
      providers: [
        {
          provide: UserService,
          useValue: {}
        },
        {
          provide: OIDCService,
          useValue: { aPIV3OIDCControllerGetOIDCProvidersGet: () => of([]) }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(LoginFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
