import { Component, EventEmitter, Input, OnInit, Output, ViewChild, ChangeDetectionStrategy } from '@angular/core';
import { DocumentPreviewDTO } from '../../../generated/open-dms-backend';
import { Observable } from 'rxjs';
import { MatTableDataSource } from '@angular/material/table';
import { MatSort } from '@angular/material/sort';

@Component({
  selector: 'app-document-table',
  standalone: false,
  templateUrl: './document-table.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './document-table.component.scss'
})
export class DocumentTableComponent implements OnInit {

  displayedColumns: string[] = ["id", "preview", "name", "importdate", "lasteditdate", "options"];

  @Input()
  documents$: Observable<DocumentPreviewDTO[]> | null = null;
  dataSource = new MatTableDataSource<DocumentPreviewDTO>([]);

  @Output()
  documentRemoved: EventEmitter<string> = new EventEmitter<string>();

  @ViewChild(MatSort) sort!: MatSort;

  ngOnInit(): void {
    if (this.documents$) {
      this.documents$.subscribe(newDocumentList => {
        this.dataSource.data = newDocumentList;
      });
    }
  }

  ngAfterViewInit(): void {
    this.dataSource.sort = this.sort;
    this.dataSource.sortingDataAccessor = (item, property) => {
      switch (property) {
        case 'id': return item.readableId ?? '';
        case 'name': return item.title ?? '';
        case 'importdate': return item.importDate ?? '';
        case 'lasteditdate': return item.versionTimestamp ?? '';
        default: return '';
      }
    };
  }

  onDocumentRemoved(documentId: string) {
    this.dataSource.data = this.dataSource.data.filter(item => item.id != documentId);
    this.documentRemoved.emit(documentId);
  }

}
