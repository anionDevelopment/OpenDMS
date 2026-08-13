import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UserAreaContainerComponent } from './user-area-container.component';
import { UserDataService } from '../../../services/user-data.service';
import { Router } from '@angular/router';
import { of } from 'rxjs';
import { UserIconComponent } from '../user-icon/user-icon.component';
import { Component, ChangeDetectionStrategy } from '@angular/core';
import { MatDividerModule } from '@angular/material/divider';
import { UtilitiesService } from '../../../services/utilities.service';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSelectModule } from '@angular/material/select';
import { MatMenuModule } from '@angular/material/menu';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatTreeModule } from '@angular/material/tree';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatInputModule } from '@angular/material/input';
import { MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatButtonModule } from '@angular/material/button';
import { MatTabsModule } from '@angular/material/tabs';
import { HomePageModule } from '../../home-page/home-page.module';

@Component({
  selector: 'app-user-icon',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.Eager,
  template: '<span>app-user-icon-container-mock</span>'
})
class UserIconComponentMock { }

describe('UserAreaContainerComponent', () => {
  let component: UserAreaContainerComponent;
  let fixture: ComponentFixture<UserAreaContainerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        CommonModule,
        ReactiveFormsModule,
        FormsModule,
        MatExpansionModule,
        MatFormFieldModule,
        MatDividerModule,
        MatCheckboxModule,
        MatSelectModule,
        MatMenuModule,
        MatPaginatorModule,
        MatTreeModule,
        MatTooltipModule,
        MatInputModule,
        MatDialogModule,
        MatIconModule,
        MatTableModule,
        MatSidenavModule,
        MatButtonModule,
        MatTabsModule,
        HomePageModule
      ],
      declarations: [
        UserIconComponentMock,
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
            parseUrl: (url: string) => new Router().parseUrl(url),
          },
        },
        {
          provide: UtilitiesService,
          useValue: new UtilitiesService()
        }
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
