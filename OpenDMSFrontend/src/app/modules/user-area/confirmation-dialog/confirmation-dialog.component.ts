import { Component, ChangeDetectionStrategy } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';

@Component({
  selector: 'app-confirmation-dialog',
  standalone: false,
  templateUrl: './confirmation-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './confirmation-dialog.component.scss'
})
export class ConfirmationDialogComponent {

  constructor(public dialogRef: MatDialogRef<ConfirmationDialogComponent>) {
  }

  close(userConfirmedAction: boolean): void {
    this.dialogRef.close(userConfirmedAction);
  }
}
