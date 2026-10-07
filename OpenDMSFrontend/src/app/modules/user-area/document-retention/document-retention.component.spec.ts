import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DocumentRetentionComponent } from './document-retention.component';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { OpenDMSBackendService } from '../../../generated/open-dms-backend';
import { StorageService } from '../../../services/storage.service';
import { of, throwError } from 'rxjs';
import type { Mock } from 'vitest';

//typed loosely on purpose: OpenDMSBackendService's generated methods are overloaded (body/response/events),
//and constraining the mock to the full interface makes TypeScript pick the wrong ('events') overload.
interface OpenDMSBackendServiceMock {
  aPIV3OpenDMSBackendSetRetentionDatesDocumentIdPut: Mock;
}

describe('DocumentRetentionComponent', () => {
  let component: DocumentRetentionComponent;
  let fixture: ComponentFixture<DocumentRetentionComponent>;
  let openDMSBackendServiceSpy: OpenDMSBackendServiceMock;

  beforeEach(async () => {
    openDMSBackendServiceSpy = {
      aPIV3OpenDMSBackendSetRetentionDatesDocumentIdPut: vi.fn(),
    };

    await TestBed.configureTestingModule({
      imports: [
        CommonModule,
        ReactiveFormsModule,
        FormsModule,
        NoopAnimationsModule,
        MatFormFieldModule,
        MatInputModule,
        MatButtonModule,
      ],
      declarations: [
        DocumentRetentionComponent,
      ],
      providers: [
        {
          provide: OpenDMSBackendService,
          useValue: openDMSBackendServiceSpy,
        },
        {
          provide: StorageService,
          useValue: {
            getAccessToken: () => 'accesstoken1',
          },
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(DocumentRetentionComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should show the dates of the document in the format of the date-input', () => {
    component.deleteIsNotAllowedBefore = '2026-01-31T12:00:00+01:00';
    component.mustBeHardDeletedAfter = '2036-01-31T12:00:00+01:00';

    component.ngOnChanges();

    expect(component.shownDeleteIsNotAllowedBefore).toBe('2026-01-31');
    expect(component.shownMustBeHardDeletedAfter).toBe('2036-01-31');
  });

  it('should show an empty input for a document without retention-dates', () => {
    component.deleteIsNotAllowedBefore = null;
    component.mustBeHardDeletedAfter = undefined;

    component.ngOnChanges();

    expect(component.shownDeleteIsNotAllowedBefore).toBe('');
    expect(component.shownMustBeHardDeletedAfter).toBe('');
  });

  it('should send the chosen dates as timestamps and report the change', () => {
    component.documentId = 'doc1';
    component.shownDeleteIsNotAllowedBefore = '2026-01-31';
    component.shownMustBeHardDeletedAfter = '';
    openDMSBackendServiceSpy.aPIV3OpenDMSBackendSetRetentionDatesDocumentIdPut.mockReturnValue(of(undefined));
    const changed = vi.fn();
    component.retentionDatesChanged.subscribe(changed);

    component.saveRetentionDates();

    expect(openDMSBackendServiceSpy.aPIV3OpenDMSBackendSetRetentionDatesDocumentIdPut).toHaveBeenCalledWith('doc1', 'accesstoken1', {
      deleteIsNotAllowedBefore: new Date('2026-01-31T00:00').toISOString(),
      mustBeHardDeletedAfter: null
    });
    expect(changed).toHaveBeenCalled();
  });

  it('should not send anything when no document is set', () => {
    component.documentId = null;

    component.saveRetentionDates();

    expect(openDMSBackendServiceSpy.aPIV3OpenDMSBackendSetRetentionDatesDocumentIdPut).not.toHaveBeenCalled();
  });

  it('should report changeNotAllowed when the change is rejected', () => {
    component.documentId = 'doc1';
    component.shownDeleteIsNotAllowedBefore = '2026-01-31';
    openDMSBackendServiceSpy.aPIV3OpenDMSBackendSetRetentionDatesDocumentIdPut.mockReturnValue(throwError(() => new Error('not allowed')));

    component.saveRetentionDates();

    expect(component.changeNotAllowed).toBe(true);
  });
});
