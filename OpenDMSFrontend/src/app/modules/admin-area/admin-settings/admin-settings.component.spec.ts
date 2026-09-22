import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Component, ChangeDetectionStrategy } from '@angular/core';
import { AdminSettingsComponent } from './admin-settings.component';
import { FrameWorkComponent } from '../../home-page/frame-work/frame-work.component';
import { AdminAreaContainerComponent } from '../admin-area-container/admin-area-container.component';
import { CommonModule } from '@angular/common';
import { HomePageModule } from '../../home-page/home-page.module';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatDividerModule } from '@angular/material/divider';
import { MatCardModule } from '@angular/material/card';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { AdminDashboardComponent } from '../admin-dashboard/admin-dashboard.component';
import { Router } from '@angular/router';
import { provideHttpClient, withXhr } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { StorageService } from '../../../services/storage.service';


//the tag-management is tested by its own testcases; here it is only replaced so that the settings-page can be rendered.
@Component({
  selector: 'app-tag-management',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.Eager,
  template: ''
})
class MockTagManagementComponent { }

describe('AdminSettingsComponent', () => {
  let component: AdminSettingsComponent;
  let fixture: ComponentFixture<AdminSettingsComponent>;

  beforeEach(async () => {
    new StorageService().setAccessToken('test-access-token');
    await TestBed.configureTestingModule({
      imports: [
        CommonModule,
        HomePageModule,
        NoopAnimationsModule,
        ReactiveFormsModule,
        FormsModule,
        MatFormFieldModule,
        MatDividerModule,
        MatCardModule,
        MatInputModule,
        MatButtonModule,
        MatIconModule,
      ],
      declarations: [
        FrameWorkComponent,
        AdminAreaContainerComponent,
        AdminDashboardComponent,
        MockTagManagementComponent,
      ],
      providers: [
        {
          provide: Router,
          useValue: {
            url: "admin/dashboard",
          },
        },
        provideHttpClient(withXhr()),
        provideHttpClientTesting(),
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AdminSettingsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach(() => {
    new StorageService().removeAccessToken();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
