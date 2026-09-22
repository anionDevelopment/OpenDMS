import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TagManagementComponent } from './tag-management.component';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { OpenDMSBackendService, TagDTO } from '../../../generated/open-dms-backend';
import { StorageService } from '../../../services/storage.service';
import { UserDataService } from '../../../services/user-data.service';
import { of, throwError } from 'rxjs';
import type { Mock } from 'vitest';

//typed loosely on purpose: OpenDMSBackendService's generated methods are overloaded (body/response/events),
//and constraining the mock to the full interface makes TypeScript pick the wrong ('events') overload.
interface OpenDMSBackendServiceMock {
  aPIV3OpenDMSBackendGetTagsPut: Mock;
  aPIV3OpenDMSBackendCreateTagPost: Mock;
  aPIV3OpenDMSBackendUpdateTagTagIdPut: Mock;
  aPIV3OpenDMSBackendDeleteTagTagIdDelete: Mock;
}

describe('TagManagementComponent', () => {
  let component: TagManagementComponent;
  let fixture: ComponentFixture<TagManagementComponent>;
  let openDMSBackendServiceSpy: OpenDMSBackendServiceMock;
  let userConfirmsTheDeletion: boolean;

  const globalTag: TagDTO = { id: 'tag-global', name: 'Invoice', colorCode: 'C62828', ownerUserId: null };
  const ownTag: TagDTO = { id: 'tag-own', name: 'Deadline', colorCode: '0277BD', ownerUserId: 'user1' };
  const tagOfAnotherUser: TagDTO = { id: 'tag-other', name: 'Private', colorCode: '2E7D32', ownerUserId: 'user2' };

  beforeEach(async () => {
    userConfirmsTheDeletion = true;
    openDMSBackendServiceSpy = {
      aPIV3OpenDMSBackendGetTagsPut: vi.fn(),
      aPIV3OpenDMSBackendCreateTagPost: vi.fn(),
      aPIV3OpenDMSBackendUpdateTagTagIdPut: vi.fn(),
      aPIV3OpenDMSBackendDeleteTagTagIdDelete: vi.fn(),
    };
    openDMSBackendServiceSpy.aPIV3OpenDMSBackendGetTagsPut.mockReturnValue(of([]));

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
        MatDialogModule,
      ],
      declarations: [
        TagManagementComponent,
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
        },
        {
          provide: UserDataService,
          useValue: {
            getUserId: () => of('user1'),
            userIsAdmin: () => of(false),
          },
        },
        {
          provide: MatDialog,
          useValue: {
            open: () => ({ afterClosed: () => of(userConfirmsTheDeletion) }),
          },
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(TagManagementComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load and alphabetically sort the tags on init', () => {
    openDMSBackendServiceSpy.aPIV3OpenDMSBackendGetTagsPut.mockReturnValue(of([ownTag, globalTag]));

    component.loadTags();

    expect(component.tags.map(tag => tag.name)).toEqual(['Deadline', 'Invoice']);
  });

  it('should only show the global tags in the admin-area, because the tags of a user are not its subject', () => {
    component.scope = 'global';
    openDMSBackendServiceSpy.aPIV3OpenDMSBackendGetTagsPut.mockReturnValue(of([ownTag, globalTag]));

    component.loadTags();

    expect(component.tags.map(tag => tag.name)).toEqual(['Invoice']);
  });

  it('should let a user manage their own tags but not the global ones', () => {
    component.scope = 'own';
    component.userIsAdministrator = true;

    expect(component.userMayManage(ownTag)).toBe(true);
    //a global tag is managed in the admin-area, so it is only shown here.
    expect(component.userMayManage(globalTag)).toBe(false);
    expect(component.userMayManage(tagOfAnotherUser)).toBe(false);
  });

  it('should let an administrator manage the global tags in the admin-area', () => {
    component.scope = 'global';
    component.userIsAdministrator = true;

    expect(component.userMayManage(globalTag)).toBe(true);
    expect(component.tagIsGlobal(globalTag)).toBe(true);
    expect(component.tagIsGlobal(ownTag)).toBe(false);
  });

  it('should not let a user who is not an administrator manage a global tag', () => {
    component.scope = 'global';
    component.userIsAdministrator = false;

    expect(component.userMayManage(globalTag)).toBe(false);
  });

  it('should create a trimmed tag which belongs to the user and reset the input', () => {
    component.scope = 'own';
    component.newTagName = '  Invoice  ';
    component.newTagColorCode = 'C62828';
    openDMSBackendServiceSpy.aPIV3OpenDMSBackendCreateTagPost.mockReturnValue(of('new-tag-id'));

    component.createTag();

    expect(openDMSBackendServiceSpy.aPIV3OpenDMSBackendCreateTagPost).toHaveBeenCalledWith('accesstoken1', { name: 'Invoice', colorCode: 'C62828', isGlobal: false });
    expect(component.newTagName).toBe('');
  });

  it('should create a global tag in the admin-area', () => {
    component.scope = 'global';
    component.newTagName = 'Invoice';
    component.newTagColorCode = 'C62828';
    openDMSBackendServiceSpy.aPIV3OpenDMSBackendCreateTagPost.mockReturnValue(of('new-tag-id'));

    component.createTag();

    expect(openDMSBackendServiceSpy.aPIV3OpenDMSBackendCreateTagPost).toHaveBeenCalledWith('accesstoken1', { name: 'Invoice', colorCode: 'C62828', isGlobal: true });
  });

  it('should not create a tag when the name is blank', () => {
    component.newTagName = '   ';

    component.createTag();

    expect(openDMSBackendServiceSpy.aPIV3OpenDMSBackendCreateTagPost).not.toHaveBeenCalled();
  });

  it('should send the new name and color of a changed tag and reload the tags', () => {
    openDMSBackendServiceSpy.aPIV3OpenDMSBackendUpdateTagTagIdPut.mockReturnValue(of(undefined));
    component.startChangingTag(ownTag);
    component.newNameOfTagBeingChanged = '  Receipt  ';
    component.newColorCodeOfTagBeingChanged = '2E7D32';

    component.saveChangedTag();

    expect(openDMSBackendServiceSpy.aPIV3OpenDMSBackendUpdateTagTagIdPut).toHaveBeenCalledWith('tag-own', 'accesstoken1', { name: 'Receipt', colorCode: '2E7D32' });
    expect(component.tagIsBeingChanged(ownTag)).toBe(false);
    expect(openDMSBackendServiceSpy.aPIV3OpenDMSBackendGetTagsPut).toHaveBeenCalled();
  });

  it('should report changeNotAllowed when changing a tag is rejected', () => {
    openDMSBackendServiceSpy.aPIV3OpenDMSBackendUpdateTagTagIdPut.mockReturnValue(throwError(() => new Error('not allowed')));
    component.startChangingTag(globalTag);
    component.newNameOfTagBeingChanged = 'Receipt';

    component.saveChangedTag();

    expect(component.changeNotAllowed).toBe(true);
  });

  it('should delete a tag after the user confirmed it', () => {
    openDMSBackendServiceSpy.aPIV3OpenDMSBackendDeleteTagTagIdDelete.mockReturnValue(of(undefined));

    component.deleteTag(ownTag);

    expect(openDMSBackendServiceSpy.aPIV3OpenDMSBackendDeleteTagTagIdDelete).toHaveBeenCalledWith('tag-own', 'accesstoken1');
  });

  it('should not delete a tag when the user does not confirm it', () => {
    userConfirmsTheDeletion = false;

    component.deleteTag(ownTag);

    expect(openDMSBackendServiceSpy.aPIV3OpenDMSBackendDeleteTagTagIdDelete).not.toHaveBeenCalled();
  });

  it('should prefix the color-code with the number-sign which css requires', () => {
    expect(component.backgroundColorOf(globalTag)).toBe('#C62828');
  });

  it('should write the name of a dark tag in a bright color and the name of a bright tag in a dark color', () => {
    expect(component.textColorOf({ id: 'dark', name: 'dark', colorCode: '000000', ownerUserId: null })).toBe('#ffffff');
    expect(component.textColorOf({ id: 'bright', name: 'bright', colorCode: 'FFFFFF', ownerUserId: null })).toBe('#000000');
  });
});
