import { Component, EventEmitter, Input, OnInit, Output, ChangeDetectionStrategy } from '@angular/core';
import { OpenDMSBackendService, TagCreationDTO, TagDTO } from '../../../generated/open-dms-backend';
import { StorageService } from '../../../services/storage.service';

/**
 * Shows the tags which are assigned to a document and lets the user change them: assign one of the tags the user
 * can use (the global tags and the tags the user owns), create a new own tag and assign it directly, or remove an
 * assignment again. Renaming and deleting a tag is not done here but in the tag-management of the settings-page,
 * because a tag exists independently of the documents it is assigned to.
 *
 * The component only knows the document and its currently assigned tags; it does not load the document itself.
 * Every change is reported via {@link tagsChanged} so that the owner of the document reloads it, which keeps a
 * single place responsible for the state of the document.
 *
 * Whether the user may change the tags of the document is decided by the backend. A rejected change is reported
 * via {@link changeNotAllowed}.
 */
@Component({
  selector: 'app-document-tags',
  standalone: false,
  templateUrl: './document-tags.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './document-tags.component.scss'
})
export class DocumentTagsComponent implements OnInit {

  /**
   * The colors a new tag can be given. The list is fixed on purpose: a free color-choice would need a native
   * color-picker, which every browser-engine renders with its own size, and it would additionally allow colors
   * which are unreadable in one of the two color-schemes.
   */
  public static readonly selectableColorCodes: string[] = ['C62828', 'AD1457', '6A1B9A', '283593', '0277BD', '00695C', '2E7D32', 'EF6C00', '4E342E', '424242'];

  @Input()
  documentId: string | null | undefined = null;

  @Input()
  assignedTags: TagDTO[] = [];

  @Output()
  tagsChanged: EventEmitter<void> = new EventEmitter<void>();

  availableTags: TagDTO[] = [];
  selectedTagId = '';
  newTagName = '';
  newTagColorCode: string = DocumentTagsComponent.selectableColorCodes[0];
  changeNotAllowed = false;

  public constructor(private storageService: StorageService, private openDMSBackendService: OpenDMSBackendService) {
  }

  ngOnInit(): void {
    this.loadAvailableTags();
  }

  get selectableColorCodes(): string[] {
    return DocumentTagsComponent.selectableColorCodes;
  }

  /**
   * The tags which exist but are not assigned to the document yet. Only these can be assigned, because assigning
   * an already-assigned tag is rejected by the backend.
   */
  get assignableTags(): TagDTO[] {
    const assignedIds = new Set<string>(this.assignedTags.map(tag => tag.id ?? ''));
    return this.availableTags.filter(tag => !assignedIds.has(tag.id ?? ''));
  }

  loadAvailableTags(): void {
    this.openDMSBackendService.aPIV3OpenDMSBackendGetTagsPut(this.storageService.getAccessToken()).subscribe({
      next: tags => this.availableTags = (tags ?? []).slice().sort((a, b) => (a.name ?? '').localeCompare(b.name ?? '')),
      error: () => this.availableTags = []
    });
  }

  assignSelectedTag(): void {
    this.changeNotAllowed = false;
    if (!this.documentId || this.selectedTagId.length === 0) {
      return;
    }
    this.openDMSBackendService.aPIV3OpenDMSBackendAssignTagDocumentIdTagIdPost(this.documentId, this.selectedTagId, this.storageService.getAccessToken()).subscribe({
      next: () => {
        this.selectedTagId = '';
        this.tagsChanged.emit();
      },
      error: () => this.changeNotAllowed = true
    });
  }

  createAndAssignTag(): void {
    this.changeNotAllowed = false;
    if (!this.documentId || this.newTagName.trim().length === 0) {
      return;
    }
    //a tag which is created while a document is indexed belongs to the user who creates it. Global tags are created in the tag-management of the settings-page, which only an administrator can do.
    const creation: TagCreationDTO = { name: this.newTagName.trim(), colorCode: this.newTagColorCode, isGlobal: false };
    this.openDMSBackendService.aPIV3OpenDMSBackendCreateTagPost(this.storageService.getAccessToken(), creation).subscribe({
      next: createdTagId => {
        this.newTagName = '';
        this.newTagColorCode = DocumentTagsComponent.selectableColorCodes[0];
        this.loadAvailableTags();
        this.assignCreatedTag(createdTagId);
      },
      error: () => this.changeNotAllowed = true
    });
  }

  unassignTag(tag: TagDTO): void {
    this.changeNotAllowed = false;
    if (!this.documentId || !tag.id) {
      return;
    }
    this.openDMSBackendService.aPIV3OpenDMSBackendUnassignTagDocumentIdTagIdDelete(this.documentId, tag.id, this.storageService.getAccessToken()).subscribe({
      next: () => this.tagsChanged.emit(),
      error: () => this.changeNotAllowed = true
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
    return DocumentTagsComponent.colorIsBright(tag.colorCode ?? '') ? '#000000' : '#ffffff';
  }

  private assignCreatedTag(createdTagId: string): void {
    this.openDMSBackendService.aPIV3OpenDMSBackendAssignTagDocumentIdTagIdPost(this.documentId!, createdTagId, this.storageService.getAccessToken()).subscribe({
      next: () => this.tagsChanged.emit(),
      error: () => this.changeNotAllowed = true
    });
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
