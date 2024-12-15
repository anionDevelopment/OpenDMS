import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UserAreaContainerComponent } from './user-area-container.component';
import { UserDataService } from '../../../services/user-data.service';
import { Router } from '@angular/router';
import { of } from 'rxjs';
import { UserIconComponent } from '../user-icon/user-icon.component';
import { Component } from '@angular/core';
import { MatDividerModule } from '@angular/material/divider';

@Component({
  selector: 'app-user-icon',
  standalone: false,
  template: '<span>app-user-icon-container-mock</span>'
})
class MockUserIconComponent { }

describe('UserAreaContainerComponent', () => {
  let component: UserAreaContainerComponent;
  let fixture: ComponentFixture<UserAreaContainerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        MatDividerModule,
      ],
      declarations: [
        MockUserIconComponent,
        UserAreaContainerComponent,
      ],
      providers: [
        {
          provide: UserDataService,
          useValue: {
            userIsAdmin: () => of(true),
          }
        },
        {
          provide: Router,
          useValue: {
            url: "user/documents",
          },
        },
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(UserAreaContainerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
