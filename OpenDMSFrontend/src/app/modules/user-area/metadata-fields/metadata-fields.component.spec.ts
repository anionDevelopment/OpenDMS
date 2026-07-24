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
import { MetadataFieldDefinitionDTO, OpenDMSBackendService } from '../../../generated/open-dms-backend';
import { StorageService } from '../../../services/storage.service';
import { of, throwError } from 'rxjs';

describe('MetadataFieldsComponent', () => {
  let component: MetadataFieldsComponent;
  let fixture: ComponentFixture<MetadataFieldsComponent>;
  //typed loosely on purpose: OpenDMSBackendService's generated methods are overloaded (body/response/events),
  //and constraining the spy to the full interface makes TypeScript pick the wrong ('events') overload.
  let openDMSBackendServiceSpy: jasmine.SpyObj<any>;

  beforeEach(async () => {
    openDMSBackendServiceSpy = jasmine.createSpyObj('OpenDMSBackendService', [
      'aPIV3OpenDMSBackendGetMetadataFieldsStorageLocationIdGet',
      'aPIV3OpenDMSBackendDefineMetadataFieldStorageLocationIdPost',
      'aPIV3OpenDMSBackendRemoveMetadataFieldFieldDefinitionIdDelete',
    ]);
    openDMSBackendServiceSpy.aPIV3OpenDMSBackendGetMetadataFieldsStorageLocationIdGet.and.returnValue(of([]));

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

    fixture = TestBed.createComponent(MetadataFieldsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should not query the backend on init when no storage-location is set', () => {
    // storageLocationId defaults to null, so ngOnInit (triggered by the initial detectChanges in beforeEach) must not call the backend.
    expect(openDMSBackendServiceSpy.aPIV3OpenDMSBackendGetMetadataFieldsStorageLocationIdGet).not.toHaveBeenCalled();
    expect(component.fields).toEqual([]);
  });

  it('should load and alphabetically sort the fields of the given storage-location', () => {
    const unsorted: MetadataFieldDefinitionDTO[] = [
      { id: '2', storageLocationId: 'sl1', name: 'zeta', type: 'String' },
      { id: '1', storageLocationId: 'sl1', name: 'alpha', type: 'Boolean' },
    ];
    openDMSBackendServiceSpy.aPIV3OpenDMSBackendGetMetadataFieldsStorageLocationIdGet.and.returnValue(of(unsorted));
    component.storageLocationId = 'sl1';

    component.loadFields();

    expect(openDMSBackendServiceSpy.aPIV3OpenDMSBackendGetMetadataFieldsStorageLocationIdGet).toHaveBeenCalledWith('sl1', 'accesstoken1');
    expect(component.fields.map(field => field.name)).toEqual(['alpha', 'zeta']);
  });

  it('should clear the fields when loading them fails', () => {
    component.storageLocationId = 'sl1';
    component.fields = [{ id: '1', storageLocationId: 'sl1', name: 'alpha', type: 'String' }];
    openDMSBackendServiceSpy.aPIV3OpenDMSBackendGetMetadataFieldsStorageLocationIdGet.and.returnValue(throwError(() => new Error('boom')));

    component.loadFields();

    expect(component.fields).toEqual([]);
  });

  it('should not add a field when no storage-location is set', () => {
    component.storageLocationId = null;
    component.newFieldName = 'sender';

    component.addField();

    expect(openDMSBackendServiceSpy.aPIV3OpenDMSBackendDefineMetadataFieldStorageLocationIdPost).not.toHaveBeenCalled();
  });

  it('should not add a field when the name is blank', () => {
    component.storageLocationId = 'sl1';
    component.newFieldName = '   ';

    component.addField();

    expect(openDMSBackendServiceSpy.aPIV3OpenDMSBackendDefineMetadataFieldStorageLocationIdPost).not.toHaveBeenCalled();
  });

  it('should add a trimmed field, reset the input and reload the fields on success', () => {
    component.storageLocationId = 'sl1';
    component.newFieldName = '  sender  ';
    component.newFieldType = 'Boolean';
    openDMSBackendServiceSpy.aPIV3OpenDMSBackendDefineMetadataFieldStorageLocationIdPost.and.returnValue(of('new-field-id'));
    openDMSBackendServiceSpy.aPIV3OpenDMSBackendGetMetadataFieldsStorageLocationIdGet.and.returnValue(of([]));

    component.addField();

    expect(openDMSBackendServiceSpy.aPIV3OpenDMSBackendDefineMetadataFieldStorageLocationIdPost).toHaveBeenCalledWith('sl1', 'accesstoken1', { name: 'sender', type: 'Boolean' });
    expect(component.newFieldName).toBe('');
    expect(component.newFieldType).toBe('String');
    expect(component.changeNotAllowed).toBeFalse();
    expect(openDMSBackendServiceSpy.aPIV3OpenDMSBackendGetMetadataFieldsStorageLocationIdGet).toHaveBeenCalledWith('sl1', 'accesstoken1');
  });

  it('should report changeNotAllowed and keep the input when adding a field is rejected (e.g. a non-moderator)', () => {
    component.storageLocationId = 'sl1';
    component.newFieldName = 'sender';
    openDMSBackendServiceSpy.aPIV3OpenDMSBackendDefineMetadataFieldStorageLocationIdPost.and.returnValue(throwError(() => new Error('forbidden')));

    component.addField();

    expect(component.changeNotAllowed).toBeTrue();
    expect(component.newFieldName).toBe('sender');
    expect(openDMSBackendServiceSpy.aPIV3OpenDMSBackendGetMetadataFieldsStorageLocationIdGet).not.toHaveBeenCalled();
  });

  it('should do nothing when removing a field without an id', () => {
    component.removeField({ storageLocationId: 'sl1', name: 'sender', type: 'String' } as MetadataFieldDefinitionDTO);

    expect(openDMSBackendServiceSpy.aPIV3OpenDMSBackendRemoveMetadataFieldFieldDefinitionIdDelete).not.toHaveBeenCalled();
  });

  it('should remove a field and reload the fields on success', () => {
    component.storageLocationId = 'sl1';
    openDMSBackendServiceSpy.aPIV3OpenDMSBackendRemoveMetadataFieldFieldDefinitionIdDelete.and.returnValue(of(undefined));
    openDMSBackendServiceSpy.aPIV3OpenDMSBackendGetMetadataFieldsStorageLocationIdGet.and.returnValue(of([]));

    component.removeField({ id: 'field-1', storageLocationId: 'sl1', name: 'sender', type: 'String' });

    expect(openDMSBackendServiceSpy.aPIV3OpenDMSBackendRemoveMetadataFieldFieldDefinitionIdDelete).toHaveBeenCalledWith('field-1', 'accesstoken1');
    expect(openDMSBackendServiceSpy.aPIV3OpenDMSBackendGetMetadataFieldsStorageLocationIdGet).toHaveBeenCalled();
  });

  it('should report changeNotAllowed when removing a field is rejected (e.g. a non-moderator)', () => {
    openDMSBackendServiceSpy.aPIV3OpenDMSBackendRemoveMetadataFieldFieldDefinitionIdDelete.and.returnValue(throwError(() => new Error('forbidden')));

    component.removeField({ id: 'field-1', storageLocationId: 'sl1', name: 'sender', type: 'String' });

    expect(component.changeNotAllowed).toBeTrue();
  });
});
