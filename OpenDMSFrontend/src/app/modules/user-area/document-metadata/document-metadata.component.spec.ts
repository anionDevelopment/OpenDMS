import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DocumentMetadataComponent } from './document-metadata.component';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { MetadataFieldDefinitionDTO, OpenDMSBackendService } from '../../../generated/open-dms-backend';
import { StorageService } from '../../../services/storage.service';
import { of, throwError } from 'rxjs';
import type { Mock } from 'vitest';

//typed loosely on purpose: OpenDMSBackendService's generated methods are overloaded (body/response/events),
//and constraining the mock to the full interface makes TypeScript pick the wrong ('events') overload.
type OpenDMSBackendServiceMock = {
  aPIV3OpenDMSBackendGetMetadataFieldsOfDocumentDocumentIdGet: Mock;
  aPIV3OpenDMSBackendSetDocumentMetadataValueDocumentIdFieldDefinitionIdPost: Mock;
  aPIV3OpenDMSBackendRemoveDocumentMetadataValueDocumentIdFieldDefinitionIdDelete: Mock;
};

const senderField: MetadataFieldDefinitionDTO = { id: 'field-sender', storageLocationId: 'sl1', name: 'sender', type: 'String' };
const taxRelevantField: MetadataFieldDefinitionDTO = { id: 'field-tax', storageLocationId: 'sl1', name: 'tax-relevant', type: 'Boolean' };

describe('DocumentMetadataComponent', () => {
  let component: DocumentMetadataComponent;
  let fixture: ComponentFixture<DocumentMetadataComponent>;
  let openDMSBackendServiceSpy: OpenDMSBackendServiceMock;

  beforeEach(async () => {
    openDMSBackendServiceSpy = {
      aPIV3OpenDMSBackendGetMetadataFieldsOfDocumentDocumentIdGet: vi.fn(),
      aPIV3OpenDMSBackendSetDocumentMetadataValueDocumentIdFieldDefinitionIdPost: vi.fn(),
      aPIV3OpenDMSBackendRemoveDocumentMetadataValueDocumentIdFieldDefinitionIdDelete: vi.fn(),
    };
    openDMSBackendServiceSpy.aPIV3OpenDMSBackendGetMetadataFieldsOfDocumentDocumentIdGet.mockReturnValue(of([]));

    await TestBed.configureTestingModule({
      imports: [
        CommonModule,
        ReactiveFormsModule,
        FormsModule,
        NoopAnimationsModule,
        MatFormFieldModule,
        MatInputModule,
        MatSelectModule,
      ],
      declarations: [
        DocumentMetadataComponent,
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

    fixture = TestBed.createComponent(DocumentMetadataComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should not query the backend when no document is set', () => {
    component.documentId = null;

    component.loadFields();

    expect(openDMSBackendServiceSpy.aPIV3OpenDMSBackendGetMetadataFieldsOfDocumentDocumentIdGet).not.toHaveBeenCalled();
    expect(component.entries).toEqual([]);
  });

  it('should show the fields of the document alphabetically together with its values', () => {
    component.documentId = 'doc1';
    component.metadataValues = { 'field-sender': 'Some Company GmbH' };
    openDMSBackendServiceSpy.aPIV3OpenDMSBackendGetMetadataFieldsOfDocumentDocumentIdGet.mockReturnValue(of([taxRelevantField, senderField]));

    component.loadFields();

    expect(openDMSBackendServiceSpy.aPIV3OpenDMSBackendGetMetadataFieldsOfDocumentDocumentIdGet).toHaveBeenCalledWith('doc1', 'accesstoken1');
    expect(component.entries.map(entry => entry.definition.name)).toEqual(['sender', 'tax-relevant']);
    expect(component.entries[0].value).toBe('Some Company GmbH');
  });

  it('should show an empty value for a field the document holds no value for', () => {
    component.documentId = 'doc1';
    component.metadataValues = {};
    openDMSBackendServiceSpy.aPIV3OpenDMSBackendGetMetadataFieldsOfDocumentDocumentIdGet.mockReturnValue(of([senderField]));

    component.loadFields();

    expect(component.entries[0].value).toBe('');
  });

  it('should clear the entries when loading the fields fails', () => {
    component.documentId = 'doc1';
    openDMSBackendServiceSpy.aPIV3OpenDMSBackendGetMetadataFieldsOfDocumentDocumentIdGet.mockReturnValue(throwError(() => new Error('boom')));

    component.loadFields();

    expect(component.entries).toEqual([]);
  });

  it('should store a value and report the change', () => {
    component.documentId = 'doc1';
    openDMSBackendServiceSpy.aPIV3OpenDMSBackendSetDocumentMetadataValueDocumentIdFieldDefinitionIdPost.mockReturnValue(of(undefined));
    const changed = vi.fn();
    component.metadataChanged.subscribe(changed);

    component.saveValue({ definition: senderField, value: 'Some Company GmbH' });

    expect(openDMSBackendServiceSpy.aPIV3OpenDMSBackendSetDocumentMetadataValueDocumentIdFieldDefinitionIdPost).toHaveBeenCalledWith('doc1', 'field-sender', 'accesstoken1', { value: 'Some Company GmbH' });
    expect(changed).toHaveBeenCalled();
  });

  it('should clear the value instead of storing an empty text, because holding no value is a different state', () => {
    component.documentId = 'doc1';
    openDMSBackendServiceSpy.aPIV3OpenDMSBackendRemoveDocumentMetadataValueDocumentIdFieldDefinitionIdDelete.mockReturnValue(of(undefined));

    component.saveValue({ definition: senderField, value: '' });

    expect(openDMSBackendServiceSpy.aPIV3OpenDMSBackendRemoveDocumentMetadataValueDocumentIdFieldDefinitionIdDelete).toHaveBeenCalledWith('doc1', 'field-sender', 'accesstoken1');
    expect(openDMSBackendServiceSpy.aPIV3OpenDMSBackendSetDocumentMetadataValueDocumentIdFieldDefinitionIdPost).not.toHaveBeenCalled();
  });

  it('should report changeNotAllowed when storing a value is rejected', () => {
    component.documentId = 'doc1';
    openDMSBackendServiceSpy.aPIV3OpenDMSBackendSetDocumentMetadataValueDocumentIdFieldDefinitionIdPost.mockReturnValue(throwError(() => new Error('forbidden')));
    const changed = vi.fn();
    component.metadataChanged.subscribe(changed);

    component.saveValue({ definition: senderField, value: 'Some Company GmbH' });

    expect(component.changeNotAllowed).toBe(true);
    expect(changed).not.toHaveBeenCalled();
  });

  it('should store a boolean-value in the representation the backend normalizes it to', () => {
    component.documentId = 'doc1';
    openDMSBackendServiceSpy.aPIV3OpenDMSBackendSetDocumentMetadataValueDocumentIdFieldDefinitionIdPost.mockReturnValue(of(undefined));

    component.saveValue({ definition: taxRelevantField, value: 'true' });

    expect(openDMSBackendServiceSpy.aPIV3OpenDMSBackendSetDocumentMetadataValueDocumentIdFieldDefinitionIdPost).toHaveBeenCalledWith('doc1', 'field-tax', 'accesstoken1', { value: 'true' });
  });

  it('should clear a boolean-value instead of storing it, because "not set" is a state of its own', () => {
    component.documentId = 'doc1';
    openDMSBackendServiceSpy.aPIV3OpenDMSBackendRemoveDocumentMetadataValueDocumentIdFieldDefinitionIdDelete.mockReturnValue(of(undefined));

    component.saveValue({ definition: taxRelevantField, value: '' });

    expect(openDMSBackendServiceSpy.aPIV3OpenDMSBackendRemoveDocumentMetadataValueDocumentIdFieldDefinitionIdDelete).toHaveBeenCalledWith('doc1', 'field-tax', 'accesstoken1');
  });

  it('should recognize which fields are edited as a boolean', () => {
    expect(component.fieldIsBoolean({ definition: taxRelevantField, value: 'true' })).toBe(true);
    expect(component.fieldIsBoolean({ definition: senderField, value: 'x' })).toBe(false);
  });
});
