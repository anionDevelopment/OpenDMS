import { Component, Inject, Input, ChangeDetectionStrategy } from '@angular/core';
import { DocumentDTO } from '../../../generated/open-dms-backend';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

@Component({
  selector: 'app-edit-document-dialog',
  standalone: false,
  templateUrl: './edit-document-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './edit-document-dialog.component.scss'
})
export class EditDocumentDialogComponent {

  documentTitle:string;
  constructor(@Inject(MAT_DIALOG_DATA) public data: { documentDTO: DocumentDTO|null}, private dialogRef: MatDialogRef<EditDocumentDialogComponent>) { 
    this.documentTitle=this.data.documentDTO!.title!;
  }
  public abort(){
      this.dialogRef.close({
        save:false
      }); 
  }  
  public save(){
      this.dialogRef.close({
        save:true,
        data:{
          documentTitle:this.documentTitle
        }
      }); 
  }  

}
