import { Component, Input, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { OpenDMSBackendService, TagCreationDTO, TagDTO } from '../../../generated/open-dms-backend';
import { StorageService } from '../../../services/storage.service';
import { UserDataService } from '../../../services/user-data.service';
import { ConfirmationDialogComponent } from '../confirmation-dialog/confirmation-dialog.component';

/** The kind of tags an instance of the tag-management manages. */
export type TagScope = 'own' | 'global';

/**
 * Lets tags be managed: created, renamed, given another color and deleted. A tag can be managed here and not on the
 * page of a document, because it exists independently of the documents it is assigned to. Deleting a tag also
 * removes it from every document it is assigned to, which is why it is confirmed first.
 *
 * The component is used in two places, which {@link scope} distinguishes: in the user-area a user manages the tags
 * which belong to them (the global tags are shown there as well, because the user can use them, but managing them
 * is an administrative function), and in the admin-area an administrator manages the global tags.
 *
 * Which tags may be changed is decided by the backend: a global tag only by an administrator and a tag which
 * belongs to a user only by that user. The same rule is applied here to only offer what is actually permitted.
 */
@Component({
  selector: 'app-tag-management',
  standalone: false,
  templateUrl: './tag-management.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './tag-management.component.scss'
})
export class TagManagementComponent implements OnInit {

  /**
   * The colors a tag can be given. The list is the same one the tags of a document offer, because a color which
   * can not be chosen there must not be choosable here either.
   */
  public static readonly selectableColorCodes: string[] = ['C62828', 'AD1457', '6A1B9A', '283593', '0277BD', '00695C', '2E7D32', 'EF6C00', '4E342E', '424242'];

  /** Whether this instance manages the tags which belong to the current user ("own") or the global tags ("global"). */
  @Input()
  scope: TagScope = 'own';

  tags: TagDTO[] = [];
  userIsAdministrator = false;
  userId = '';

  newTagName = '';
  newTagColorCode: string = TagManagementComponent.selectableColorCodes[0];

  /** The id of the tag which is currently being changed, or an empty string when no tag is being changed. */
  tagIdBeingChanged = '';
  newNameOfTagBeingChanged = '';
  newColorCodeOfTagBeingChanged: string = TagManagementComponent.selectableColorCodes[0];

  changeNotAllowed = false;

  public constructor(private storageService: StorageService, private openDMSBackendService: OpenDMSBackendService, private userDataService: UserDataService, private dialog: MatDialog) {
  }

  ngOnInit(): void {
    //the id of the user and whether the user is an administrator decide which tags may be changed here. Both are
    //taken from the user-data-service and not from the storage-service, because the service loads them when they
    //are not loaded yet, while the storage only holds them after that happened.
    this.userDataService.getUserId().subscribe(userId => this.userId = userId);
    this.userDataService.userIsAdmin().subscribe(userIsAdministrator => this.userIsAdministrator = userIsAdministrator);
    this.loadTags();
  }

  get selectableColorCodes(): string[] {
    return TagManagementComponent.selectableColorCodes;
  }

  loadTags(): void {
    this.openDMSBackendService.aPIV3OpenDMSBackendGetTagsPut(this.storageService.getAccessToken()).subscribe({
      next: tags => this.tags = (tags ?? []).filter(tag => this.belongsToTheShownScope(tag)).sort((a, b) => (a.name ?? '').localeCompare(b.name ?? '')),
      error: () => this.tags = []
    });
  }

  /**
   * Whether the given tag is shown by this instance. The user-area additionally shows the global tags, because a
   * user can use them; the admin-area shows the global tags only, because the tags of a user are not its subject.
   */
  private belongsToTheShownScope(tag: TagDTO): boolean {
    return this.scope === 'own' || this.tagIsGlobal(tag);
  }

  tagIsGlobal(tag: TagDTO): boolean {
    return !tag.ownerUserId;
  }

  userMayManage(tag: TagDTO): boolean {
    return this.scope === 'global'
      ? this.tagIsGlobal(tag) && this.userIsAdministrator
      : tag.ownerUserId === this.userId;
  }

  createTag(): void {
    this.changeNotAllowed = false;
    if (this.newTagName.trim().length === 0) {
      return;
    }
    const creation: TagCreationDTO = { name: this.newTagName.trim(), colorCode: this.newTagColorCode, isGlobal: this.scope === 'global' };
    this.openDMSBackendService.aPIV3OpenDMSBackendCreateTagPost(this.storageService.getAccessToken(), creation).subscribe({
      next: () => {
        this.newTagName = '';
        this.newTagColorCode = TagManagementComponent.selectableColorCodes[0];
        this.loadTags();
      },
      error: () => this.changeNotAllowed = true
    });
  }

  startChangingTag(tag: TagDTO): void {
    this.changeNotAllowed = false;
    this.tagIdBeingChanged = tag.id ?? '';
    this.newNameOfTagBeingChanged = tag.name ?? '';
    this.newColorCodeOfTagBeingChanged = tag.colorCode ?? TagManagementComponent.selectableColorCodes[0];
  }

  cancelChangingTag(): void {
    this.tagIdBeingChanged = '';
    this.newNameOfTagBeingChanged = '';
  }

  tagIsBeingChanged(tag: TagDTO): boolean {
    return this.tagIdBeingChanged.length > 0 && this.tagIdBeingChanged === tag.id;
  }

  saveChangedTag(): void {
    this.changeNotAllowed = false;
    if (this.tagIdBeingChanged.length === 0 || this.newNameOfTagBeingChanged.trim().length === 0) {
      return;
    }
    this.openDMSBackendService.aPIV3OpenDMSBackendUpdateTagTagIdPut(this.tagIdBeingChanged, this.storageService.getAccessToken(), { name: this.newNameOfTagBeingChanged.trim(), colorCode: this.newColorCodeOfTagBeingChanged }).subscribe({
      next: () => {
        this.cancelChangingTag();
        this.loadTags();
      },
      error: () => this.changeNotAllowed = true
    });
  }

  deleteTag(tag: TagDTO): void {
    this.changeNotAllowed = false;
    if (!tag.id) {
      return;
    }
    //deleting a tag also removes it from every document it is assigned to, so the user confirms it first.
    this.dialog.open(ConfirmationDialogComponent).afterClosed().subscribe(userConfirmedAction => {
      if (!userConfirmedAction) {
        return;
      }
      this.openDMSBackendService.aPIV3OpenDMSBackendDeleteTagTagIdDelete(tag.id!, this.storageService.getAccessToken()).subscribe({
        next: () => {
          this.cancelChangingTag();
          this.loadTags();
        },
        error: () => this.changeNotAllowed = true
      });
    });
  }

  /**
   * The background-color of a tag as a css-color. The backend delivers the six hexadecimal digits without the
   * number-sign which css requires.
   */
  backgroundColorOf(tag: TagDTO): string {
    return '#' + (tag.colorCode ?? '');
  }

  /**
   * The color the name of a tag is written in. A dark tag-color needs a bright text and a bright tag-color needs a
   * dark one, so that the name stays readable whichever color the tag was given.
   */
  textColorOf(tag: TagDTO): string {
    return TagManagementComponent.colorIsBright(tag.colorCode ?? '') ? '#000000' : '#ffffff';
  }

  /**
   * Whether the given six-digit hexadecimal rgb-value is a bright color, measured by the perceived brightness of
   * its three channels (the human eye perceives green much brighter than blue at the same value).
   */
  private static colorIsBright(colorCode: string): boolean {
    const hexadecimalValue: string = colorCode.replace('#', '');
    if (hexadecimalValue.length !== 6) {
      return false;
    }
    const red: number = parseInt(hexadecimalValue.substring(0, 2), 16);
    const green: number = parseInt(hexadecimalValue.substring(2, 4), 16);
    const blue: number = parseInt(hexadecimalValue.substring(4, 6), 16);
    return (red * 299 + green * 587 + blue * 114) / 1000 > 140;
  }
}
