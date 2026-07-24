import { Component } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { Observable, combineLatest, iif, of, switchMap } from 'rxjs';
import { UserDataService } from '../../../services/user-data.service';

@Component({
  selector: 'app-user-dashboard',
  standalone: false,
  templateUrl: './user-dashboard.component.html',
  styleUrl: './user-dashboard.component.scss'
})
export class UserDashboardComponent {
  userName: string | null = null;
  constructor(userDataService: UserDataService) {
    userDataService.getUserName().subscribe(userName => {
      this.userName = userName;
    });
  }
}
