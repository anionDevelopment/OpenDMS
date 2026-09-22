import { Component, Input, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { MetadataFieldDefinitionCreationDTO, MetadataFieldDefinitionDTO, OpenDMSBackendService } from '../../../generated/open-dms-backend';
import { StorageService } from '../../../services/storage.service';

/**
 * Displays the custom metadata-fields defined for a storage-location and lets a moderator add, rename or remove
 * them. The list is shown to every user which may view the storage-location; changing is only permitted for
 * moderators and is enforced by the backend (a rejected change is reported via {@link changeNotAllowed}).
 *
 * The type of a field can not be changed after it was defined, because the values which the documents already
 * hold for it were validated against it.
 */
@Component({
  selector: 'app-metadata-fields',
  standalone: false,
  templateUrl: './metadata-fields.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './metadata-fields.component.scss'
})
export class MetadataFieldsComponent implements OnInit {

  @Input()
  storageLocationId: string | null | undefined = null;

  @Input()
  userIsModerator = true;//TODO set initial value to false and set only to true when the user is a moderator of the storage-location

  fields: MetadataFieldDefinitionDTO[] = [];
  newFieldName = '';
  newFieldType = 'String';
  changeNotAllowed = false;

  /** The id of the field which is currently being renamed, or an empty string when no field is being renamed. */
  fieldIdBeingRenamed = '';
  /** The name which the field that is currently being renamed will get. */
  newNameOfFieldBeingRenamed = '';

  public constructor(private storageService: StorageService, private openDMSBackendService: OpenDMSBackendService) {
  }

  ngOnInit(): void {
    this.loadFields();
  }

  loadFields(): void {
    if (!this.storageLocationId) {
      return;
    }
    this.openDMSBackendService.aPIV3OpenDMSBackendGetMetadataFieldsStorageLocationIdGet(this.storageLocationId, this.storageService.getAccessToken()).subscribe({
      next: fields => {
        this.fields = (fields ?? []).sort((a, b) => (a.name ?? '').localeCompare(b.name ?? ''));
      },
      error: () => this.fields = []
    });
  }

  addField(): void {
    this.changeNotAllowed = false;
    if (!this.storageLocationId || this.newFieldName.trim().length === 0) {
      return;
    }
    const creation: MetadataFieldDefinitionCreationDTO = { name: this.newFieldName.trim(), type: this.newFieldType };
    this.openDMSBackendService.aPIV3OpenDMSBackendDefineMetadataFieldStorageLocationIdPost(this.storageLocationId, this.storageService.getAccessToken(), creation).subscribe({
      next: () => {
        this.newFieldName = '';
        this.newFieldType = 'String';
        this.loadFields();
      },
      error: () => this.changeNotAllowed = true
    });
  }

  startRenamingField(field: MetadataFieldDefinitionDTO): void {
    this.changeNotAllowed = false;
    this.fieldIdBeingRenamed = field.id ?? '';
    this.newNameOfFieldBeingRenamed = field.name ?? '';
  }

  cancelRenamingField(): void {
    this.fieldIdBeingRenamed = '';
    this.newNameOfFieldBeingRenamed = '';
  }

  fieldIsBeingRenamed(field: MetadataFieldDefinitionDTO): boolean {
    return this.fieldIdBeingRenamed.length > 0 && this.fieldIdBeingRenamed === field.id;
  }

  renameField(): void {
    this.changeNotAllowed = false;
    if (this.fieldIdBeingRenamed.length === 0 || this.newNameOfFieldBeingRenamed.trim().length === 0) {
      return;
    }
    this.openDMSBackendService.aPIV3OpenDMSBackendRenameMetadataFieldFieldDefinitionIdPut(this.fieldIdBeingRenamed, this.storageService.getAccessToken(), { value: this.newNameOfFieldBeingRenamed.trim() }).subscribe({
      next: () => {
        this.cancelRenamingField();
        this.loadFields();
      },
      error: () => this.changeNotAllowed = true
    });
  }

  removeField(field: MetadataFieldDefinitionDTO): void {
    this.changeNotAllowed = false;
    if (!field.id) {
      return;
    }
    this.openDMSBackendService.aPIV3OpenDMSBackendRemoveMetadataFieldFieldDefinitionIdDelete(field.id, this.storageService.getAccessToken()).subscribe({
      next: () => {
        this.cancelRenamingField();
        this.loadFields();
      },
      error: () => this.changeNotAllowed = true
    });
  }
}
