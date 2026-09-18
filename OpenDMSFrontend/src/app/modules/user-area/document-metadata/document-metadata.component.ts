import { Component, EventEmitter, Input, OnChanges, Output, ChangeDetectionStrategy } from '@angular/core';
import { MetadataFieldDefinitionDTO, OpenDMSBackendService } from '../../../generated/open-dms-backend';
import { StorageService } from '../../../services/storage.service';

/**
 * A single custom metadata-field together with the value the shown document holds for it.
 *
 * A boolean-field offers three values and not two, because a document which was never indexed by that field is in
 * a different state than a document for which the field was answered with "no"; a checkbox could not express that
 * difference. The two answers are the lower-case representation which the backend normalizes a boolean-value to.
 */
export interface DocumentMetadataEntry {
  definition: MetadataFieldDefinitionDTO;
  /** The value of the document as it is currently shown in the user-interface. An empty string means that the document holds no value for this field. */
  value: string;
}

/**
 * Shows the custom metadata-fields which are defined for the storage-location of a document, together with the
 * value the document holds for each of them, and lets the user index the document by changing those values.
 *
 * The component loads the field-definitions itself, because they belong to the storage-location and not to the
 * document. The values belong to the document and are therefore passed in and reported back via
 * {@link metadataChanged}, so that the owner of the document stays the single place responsible for its state.
 *
 * Whether the user may change the document is decided by the backend. A rejected change is reported via
 * {@link changeNotAllowed}.
 */
@Component({
  selector: 'app-document-metadata',
  standalone: false,
  templateUrl: './document-metadata.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './document-metadata.component.scss'
})
export class DocumentMetadataComponent implements OnChanges {

  @Input()
  documentId: string | null | undefined = null;

  @Input()
  metadataValues: { [fieldDefinitionId: string]: string } | null | undefined = null;

  @Output()
  metadataChanged: EventEmitter<void> = new EventEmitter<void>();

  entries: DocumentMetadataEntry[] = [];
  changeNotAllowed: boolean = false;

  public constructor(private storageService: StorageService, private openDMSBackendService: OpenDMSBackendService) {
  }

  ngOnChanges(): void {
    this.loadFields();
  }

  loadFields(): void {
    if (!this.documentId) {
      this.entries = [];
      return;
    }
    this.openDMSBackendService.aPIV3OpenDMSBackendGetMetadataFieldsOfDocumentDocumentIdGet(this.documentId, this.storageService.getAccessToken()).subscribe({
      next: fields => this.entries = this.toEntries(fields ?? []),
      error: () => this.entries = []
    });
  }

  fieldIsBoolean(entry: DocumentMetadataEntry): boolean {
    return entry.definition.type === 'Boolean';
  }

  /**
   * Stores the value which is currently shown for the given field. An empty value means that the document holds no
   * value for the field, which is a different state than holding an empty text and is therefore cleared instead of
   * being stored.
   */
  saveValue(entry: DocumentMetadataEntry): void {
    this.changeNotAllowed = false;
    if (!this.documentId || !entry.definition.id) {
      return;
    }
    const request = entry.value.length === 0
      ? this.openDMSBackendService.aPIV3OpenDMSBackendRemoveDocumentMetadataValueDocumentIdFieldDefinitionIdDelete(this.documentId, entry.definition.id, this.storageService.getAccessToken())
      : this.openDMSBackendService.aPIV3OpenDMSBackendSetDocumentMetadataValueDocumentIdFieldDefinitionIdPost(this.documentId, entry.definition.id, this.storageService.getAccessToken(), { value: entry.value });
    request.subscribe({
      next: () => this.metadataChanged.emit(),
      error: () => this.changeNotAllowed = true
    });
  }

  private toEntries(fields: MetadataFieldDefinitionDTO[]): DocumentMetadataEntry[] {
    const values: { [fieldDefinitionId: string]: string } = this.metadataValues ?? {};
    return fields
      .slice()
      .sort((a, b) => (a.name ?? '').localeCompare(b.name ?? ''))
      .map(definition => ({ definition: definition, value: values[definition.id ?? ''] ?? '' }));
  }
}
