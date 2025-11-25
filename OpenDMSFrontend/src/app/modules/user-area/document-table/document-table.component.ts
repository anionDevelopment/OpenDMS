import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { DocumentPreviewDTO } from '../../../generated/open-dms-backend';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-document-table',
  standalone: false,
  templateUrl: './document-table.component.html',
  styleUrl: './document-table.component.scss'
})
export class DocumentTableComponent implements OnInit {

  displayedColumns: string[] = ["id", "name", "importdate", "options"];

  @Input()
  documents$: Observable<DocumentPreviewDTO[]> | null = null;
  documents: DocumentPreviewDTO[] = [];

  @Output()
  documentRemoved: EventEmitter<string/*document-id*/> = new EventEmitter<string>();

  ngOnInit(): void {
    if (this.documents$) {
      this.documents$.subscribe(newDocumentList => {
        this.documents = newDocumentList;
      });
    }
  }

  onDocumentRemoved(documentId: string) {
    const documents = this.documents.filter(item => item.id != documentId);
    this.documents = [...documents];
    this.documentRemoved.emit(documentId);
  }

}
