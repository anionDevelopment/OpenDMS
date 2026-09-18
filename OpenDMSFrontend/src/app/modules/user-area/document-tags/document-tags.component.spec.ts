import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DocumentTagsComponent } from './document-tags.component';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { OpenDMSBackendService, TagDTO } from '../../../generated/open-dms-backend';
import { StorageService } from '../../../services/storage.service';
import { of, throwError } from 'rxjs';
import type { Mock } from 'vitest';

//typed loosely on purpose: OpenDMSBackendService's generated methods are overloaded (body/response/events),
//and constraining the mock to the full interface makes TypeScript pick the wrong ('events') overload.
type OpenDMSBackendServiceMock = {
  aPIV3OpenDMSBackendGetAllTagsPut: Mock;
  aPIV3OpenDMSBackendCreateTagPost: Mock;
  aPIV3OpenDMSBackendAssignTagDocumentIdTagIdPost: Mock;
  aPIV3OpenDMSBackendUnassignTagDocumentIdTagIdDelete: Mock;
};

describe('DocumentTagsComponent', () => {
  let component: DocumentTagsComponent;
  let fixture: ComponentFixture<DocumentTagsComponent>;
  let openDMSBackendServiceSpy: OpenDMSBackendServiceMock;

  beforeEach(async () => {
    openDMSBackendServiceSpy = {
      aPIV3OpenDMSBackendGetAllTagsPut: vi.fn(),
      aPIV3OpenDMSBackendCreateTagPost: vi.fn(),
      aPIV3OpenDMSBackendAssignTagDocumentIdTagIdPost: vi.fn(),
      aPIV3OpenDMSBackendUnassignTagDocumentIdTagIdDelete: vi.fn(),
    };
    openDMSBackendServiceSpy.aPIV3OpenDMSBackendGetAllTagsPut.mockReturnValue(of([]));

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
        DocumentTagsComponent,
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

    fixture = TestBed.createComponent(DocumentTagsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load and alphabetically sort all existing tags on init', () => {
    const unsorted: TagDTO[] = [
      { id: '2', name: 'zeta', colorCode: 'C62828' },
      { id: '1', name: 'alpha', colorCode: '283593' },
    ];
    openDMSBackendServiceSpy.aPIV3OpenDMSBackendGetAllTagsPut.mockReturnValue(of(unsorted));

    component.loadAvailableTags();

    expect(component.availableTags.map(tag => tag.name)).toEqual(['alpha', 'zeta']);
  });

  it('should clear the available tags when loading them fails', () => {
    component.availableTags = [{ id: '1', name: 'alpha', colorCode: '283593' }];
    openDMSBackendServiceSpy.aPIV3OpenDMSBackendGetAllTagsPut.mockReturnValue(throwError(() => new Error('boom')));

    component.loadAvailableTags();

    expect(component.availableTags).toEqual([]);
  });

  it('should only offer tags which are not assigned to the document yet', () => {
    component.availableTags = [
      { id: '1', name: 'alpha', colorCode: '283593' },
      { id: '2', name: 'zeta', colorCode: 'C62828' },
    ];
    component.assignedTags = [{ id: '1', name: 'alpha', colorCode: '283593' }];

    expect(component.assignableTags.map(tag => tag.id)).toEqual(['2']);
  });

  it('should not assign a tag when no document is set', () => {
    component.documentId = null;
    component.selectedTagId = '1';

    component.assignSelectedTag();

    expect(openDMSBackendServiceSpy.aPIV3OpenDMSBackendAssignTagDocumentIdTagIdPost).not.toHaveBeenCalled();
  });

  it('should not assign a tag when none is selected', () => {
    component.documentId = 'doc1';
    component.selectedTagId = '';

    component.assignSelectedTag();

    expect(openDMSBackendServiceSpy.aPIV3OpenDMSBackendAssignTagDocumentIdTagIdPost).not.toHaveBeenCalled();
  });

  it('should assign the selected tag, reset the selection and report the change', () => {
    component.documentId = 'doc1';
    component.selectedTagId = 'tag1';
    openDMSBackendServiceSpy.aPIV3OpenDMSBackendAssignTagDocumentIdTagIdPost.mockReturnValue(of(undefined));
    const changed = vi.fn();
    component.tagsChanged.subscribe(changed);

    component.assignSelectedTag();

    expect(openDMSBackendServiceSpy.aPIV3OpenDMSBackendAssignTagDocumentIdTagIdPost).toHaveBeenCalledWith('doc1', 'tag1', 'accesstoken1');
    expect(component.selectedTagId).toBe('');
    expect(changed).toHaveBeenCalled();
  });

  it('should report changeNotAllowed when assigning a tag is rejected', () => {
    component.documentId = 'doc1';
    component.selectedTagId = 'tag1';
    openDMSBackendServiceSpy.aPIV3OpenDMSBackendAssignTagDocumentIdTagIdPost.mockReturnValue(throwError(() => new Error('forbidden')));
    const changed = vi.fn();
    component.tagsChanged.subscribe(changed);

    component.assignSelectedTag();

    expect(component.changeNotAllowed).toBe(true);
    expect(changed).not.toHaveBeenCalled();
  });

  it('should not create a tag when the name is blank', () => {
    component.documentId = 'doc1';
    component.newTagName = '   ';

    component.createAndAssignTag();

    expect(openDMSBackendServiceSpy.aPIV3OpenDMSBackendCreateTagPost).not.toHaveBeenCalled();
  });

  it('should create a trimmed tag, assign it directly and reset the input', () => {
    component.documentId = 'doc1';
    component.newTagName = '  Invoice  ';
    component.newTagColorCode = 'C62828';
    openDMSBackendServiceSpy.aPIV3OpenDMSBackendCreateTagPost.mockReturnValue(of('new-tag-id'));
    openDMSBackendServiceSpy.aPIV3OpenDMSBackendAssignTagDocumentIdTagIdPost.mockReturnValue(of(undefined));
    const changed = vi.fn();
    component.tagsChanged.subscribe(changed);

    component.createAndAssignTag();

    expect(openDMSBackendServiceSpy.aPIV3OpenDMSBackendCreateTagPost).toHaveBeenCalledWith('accesstoken1', { name: 'Invoice', colorCode: 'C62828' });
    expect(openDMSBackendServiceSpy.aPIV3OpenDMSBackendAssignTagDocumentIdTagIdPost).toHaveBeenCalledWith('doc1', 'new-tag-id', 'accesstoken1');
    expect(component.newTagName).toBe('');
    expect(changed).toHaveBeenCalled();
  });

  it('should report changeNotAllowed and keep the input when creating a tag is rejected', () => {
    component.documentId = 'doc1';
    component.newTagName = 'Invoice';
    openDMSBackendServiceSpy.aPIV3OpenDMSBackendCreateTagPost.mockReturnValue(throwError(() => new Error('duplicate')));

    component.createAndAssignTag();

    expect(component.changeNotAllowed).toBe(true);
    expect(component.newTagName).toBe('Invoice');
    expect(openDMSBackendServiceSpy.aPIV3OpenDMSBackendAssignTagDocumentIdTagIdPost).not.toHaveBeenCalled();
  });

  it('should unassign a tag and report the change', () => {
    component.documentId = 'doc1';
    openDMSBackendServiceSpy.aPIV3OpenDMSBackendUnassignTagDocumentIdTagIdDelete.mockReturnValue(of(undefined));
    const changed = vi.fn();
    component.tagsChanged.subscribe(changed);

    component.unassignTag({ id: 'tag1', name: 'Invoice', colorCode: 'C62828' });

    expect(openDMSBackendServiceSpy.aPIV3OpenDMSBackendUnassignTagDocumentIdTagIdDelete).toHaveBeenCalledWith('doc1', 'tag1', 'accesstoken1');
    expect(changed).toHaveBeenCalled();
  });

  it('should do nothing when unassigning a tag without an id', () => {
    component.documentId = 'doc1';

    component.unassignTag({ name: 'Invoice', colorCode: 'C62828' });

    expect(openDMSBackendServiceSpy.aPIV3OpenDMSBackendUnassignTagDocumentIdTagIdDelete).not.toHaveBeenCalled();
  });

  it('should write the name of a dark tag in a bright color and the name of a bright tag in a dark color', () => {
    expect(component.textColorOf({ id: '1', name: 'dark', colorCode: '283593' })).toBe('#ffffff');
    expect(component.textColorOf({ id: '2', name: 'bright', colorCode: 'FFEB3B' })).toBe('#000000');
  });

  it('should prefix the color-code with the number-sign which css requires', () => {
    expect(component.backgroundColorOf({ id: '1', name: 'alpha', colorCode: 'C62828' })).toBe('#C62828');
  });
});
