import { Component, EventEmitter, Input, OnChanges, Output, ChangeDetectionStrategy } from '@angular/core';
import { OpenDMSBackendService } from '../../../generated/open-dms-backend';
import { StorageService } from '../../../services/storage.service';

/**
 * Shows the retention-dates of a document and lets the user set them.
 *
 * As long as the date before which the document must not be deleted is in the future, the backend refuses to
 * hard-delete the document; when the date after which it must be deleted is reached, the scheduled housekeeping
 * hard-deletes it. Both dates are optional: a document without them is neither protected nor deleted automatically.
 *
 * The dates belong to the document and are therefore passed in and reported back via {@link retentionDatesChanged},
 * so that the owner of the document stays the single place responsible for its state. Whether the user may change
 * the document is decided by the backend; a rejected change is reported via {@link changeNotAllowed}.
 */
@Component({
  selector: 'app-document-retention',
  standalone: false,
  templateUrl: './document-retention.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './document-retention.component.scss'
})
export class DocumentRetentionComponent implements OnChanges {

  @Input()
  documentId: string | null | undefined = null;

  @Input()
  deleteIsNotAllowedBefore: string | null | undefined = null;

  @Input()
  mustBeHardDeletedAfter: string | null | undefined = null;

  @Output()
  retentionDatesChanged: EventEmitter<void> = new EventEmitter<void>();

  /** The shown date before which the document must not be deleted, in the format of the date-input ("yyyy-MM-dd"). */
  shownDeleteIsNotAllowedBefore = '';
  /** The shown date after which the document must be deleted, in the format of the date-input ("yyyy-MM-dd"). */
  shownMustBeHardDeletedAfter = '';
  changeNotAllowed = false;

  public constructor(private storageService: StorageService, private openDMSBackendService: OpenDMSBackendService) {
  }

  ngOnChanges(): void {
    this.shownDeleteIsNotAllowedBefore = DocumentRetentionComponent.toShownDate(this.deleteIsNotAllowedBefore);
    this.shownMustBeHardDeletedAfter = DocumentRetentionComponent.toShownDate(this.mustBeHardDeletedAfter);
    this.changeNotAllowed = false;
  }

  saveRetentionDates(): void {
    this.changeNotAllowed = false;
    if (!this.documentId) {
      return;
    }
    this.openDMSBackendService.aPIV3OpenDMSBackendSetRetentionDatesDocumentIdPut(this.documentId, this.storageService.getAccessToken(), {
      deleteIsNotAllowedBefore: DocumentRetentionComponent.toBackendTimestamp(this.shownDeleteIsNotAllowedBefore),
      mustBeHardDeletedAfter: DocumentRetentionComponent.toBackendTimestamp(this.shownMustBeHardDeletedAfter)
    }).subscribe({
      next: () => this.retentionDatesChanged.emit(),
      error: () => this.changeNotAllowed = true
    });
  }

  /** The date as the date-input shows it. A date which the backend does not deliver is shown as an empty input. */
  private static toShownDate(storedValue: string | null | undefined): string {
    if (!storedValue) {
      return '';
    }
    const timestamp: Date = new Date(storedValue);
    if (isNaN(timestamp.getTime())) {
      return '';
    }
    const twoDigits: (value: number) => string = value => value.toString().padStart(2, '0');
    return `${timestamp.getFullYear()}-${twoDigits(timestamp.getMonth() + 1)}-${twoDigits(timestamp.getDate())}`;
  }

  /**
   * The value of a date-input as the iso-8601-timestamp which the backend expects, or null when no date is set.
   * The date is sent as the beginning of that day in the local time-zone of the user, which is the day the user chose.
   */
  private static toBackendTimestamp(shownDate: string): string | null {
    if (shownDate.length === 0) {
      return null;
    }
    const timestamp: Date = new Date(`${shownDate}T00:00`);
    if (isNaN(timestamp.getTime())) {
      return null;
    }
    return timestamp.toISOString();
  }
}
