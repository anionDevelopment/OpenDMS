import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MetadataFieldsComponent } from './metadata-fields.component';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { OpenDMSBackendService } from '../../../generated/open-dms-backend';

describe('MetadataFieldsComponent', () => {
  let component: MetadataFieldsComponent;
  let fixture: ComponentFixture<MetadataFieldsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        CommonModule,
        ReactiveFormsModule,
        FormsModule,
        NoopAnimationsModule,
        MatFormFieldModule,
        MatSelectModule,
        MatInputModule,
        MatIconModule,
        MatButtonModule,
      ],
      declarations: [
        MetadataFieldsComponent,
      ],
      providers: [
        {
          provide: OpenDMSBackendService,
          useValue: {
          },
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(MetadataFieldsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
