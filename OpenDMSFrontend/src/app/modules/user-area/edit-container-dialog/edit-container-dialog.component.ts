import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

@Component({
  selector: 'app-edit-container-dialog',
  standalone: false,
  templateUrl: './edit-container-dialog.component.html',
  styleUrl: './edit-container-dialog.component.scss'
})
export class EditContainerDialogComponent {

  containerTitle: string;

  constructor(@Inject(MAT_DIALOG_DATA) public data: { containerTitle: string }, private dialogRef: MatDialogRef<EditContainerDialogComponent>) {
    this.containerTitle = this.data.containerTitle;
  }

  public abort(): void {
    this.dialogRef.close({ save: false });
  }

  public save(): void {
    this.dialogRef.close({
      save: true,
      data: { containerTitle: this.containerTitle }
    });
  }
}
