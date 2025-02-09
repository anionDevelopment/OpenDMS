import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UserDashboardComponent } from './user-dashboard.component';
import { UserDataService } from '../../../services/user-data.service';
import { of } from 'rxjs';
import { HomePageModule } from '../../home-page/home-page.module';
import { Component } from '@angular/core';

@Component({
  selector: 'app-user-area-container',
  standalone: false,
  template: '<span>app-user-area-container-mock</span>'
})
class MockUserAreaContainerComponent { }

describe('UserDashboardComponent', () => {
  let component: UserDashboardComponent;
  let fixture: ComponentFixture<UserDashboardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        HomePageModule,
      ],
      declarations: [
        MockUserAreaContainerComponent,
        UserDashboardComponent,
      ],
      providers: [
        {
          provide: UserDataService,
          useValue: {
            getUserName: () => of("username"),
          }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(UserDashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
