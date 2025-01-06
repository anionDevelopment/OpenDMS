import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DocumentComponent } from './document.component';
import { HomePageModule } from '../../home-page/home-page.module';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { of } from 'rxjs';
import { UserAreaContainerComponent } from '../user-area-container/user-area-container.component';
import { Component } from '@angular/core';

@Component({
  selector: 'app-user-area-container',
  standalone: false,
  template: '<ng-content></ng-content>'
})
class UserAreaContainerComponentMock { }

describe('DocumentComponent', () => {
  let component: DocumentComponent;
  let fixture: ComponentFixture<DocumentComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        CommonModule,
        HomePageModule,
      ],
      declarations: [
        UserAreaContainerComponentMock,
        DocumentComponent,
      ],
      providers: [
        {
          provide: ActivatedRoute,
          useValue: {
            queryParams: of({
              "documentId": "documentId"
            })
          },
        },
        {
          provide: HttpClient,
          useValue: {
          },
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(DocumentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
