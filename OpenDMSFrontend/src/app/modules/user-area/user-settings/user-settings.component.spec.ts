import { ComponentFixture, TestBed } from '@angular/core/testing';
import { UserSettingsComponent } from './user-settings.component';
import { CommonModule } from '@angular/common';
import { MatDividerModule } from '@angular/material/divider';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { HomePageModule } from '../../home-page/home-page.module';
import { MatIconModule } from '@angular/material/icon';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatButtonModule } from '@angular/material/button';
import { MatTabsModule } from '@angular/material/tabs';
import { UserAreaContainerComponent } from '../user-area-container/user-area-container.component';
import { UserService } from '../../../generated/open-dms-backend';
import { Router } from '@angular/router';
import { StorageService } from '../../../services/storage.service';
import { of } from 'rxjs';
import { UserIconComponent } from '../user-icon/user-icon.component';
import { Component } from '@angular/core';

@Component({
  selector: 'app-user-area-container',
  standalone: false,
  template: '<ng-content></ng-content>'
})
class UserAreaContainerComponentMock { }

describe('UserSettingsComponent', () => {
  let component: UserSettingsComponent;
  let fixture: ComponentFixture<UserSettingsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        CommonModule,
        MatDividerModule,
        MatMenuModule,
        MatTooltipModule,
        HomePageModule,
        MatIconModule,
        MatSidenavModule,
        MatButtonModule,
        MatTabsModule,
      ],
      declarations: [
        UserIconComponent,
        UserAreaContainerComponentMock,
        UserSettingsComponent,
      ],
      providers: [
        {
          provide: StorageService,
          useValue: {
            getAccessToken: () => "accesstoken1",
          }
        },
        {
          provide: UserService,
          useValue: {
            aPIV3UserControllerGetUserInformationGet: (token: string) => of({
              id: "id1",
              name: "admin",
              isAdmin: true,
            })
          }
        },
        {
          provide: Router,
          useValue: {
            url: 'user/settings'
          }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(UserSettingsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
