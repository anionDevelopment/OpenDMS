import { Component } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { DocumentPreviewDTO, OpenDMSBackendService } from '../../../generated/open-dms-backend';
import { StorageService } from '../../../services/storage.service';

@Component({
  selector: 'app-search',
  standalone: false,
  templateUrl: './search.component.html',
  styleUrl: './search.component.scss'
})
export class SearchComponent {
  documents$: BehaviorSubject<DocumentPreviewDTO[]> = new BehaviorSubject<DocumentPreviewDTO[]>([]);
  searchText: string = "";

  constructor(private storageService: StorageService, private openDMSBackendService: OpenDMSBackendService) {
  }

  search() {
    console.log("log");
    this.openDMSBackendService.aPIV1OpenDMSBackendSearchGet(this.storageService.getAccessToken(), this.searchText).subscribe((result => {
      this.documents$.next(result);
    }));
  }

  onDocumentRemoved(documentId: string) {
    const documents = this.documents$.value.filter(document => document.id != documentId);
    this.documents$.next([...documents]);
  }
}
