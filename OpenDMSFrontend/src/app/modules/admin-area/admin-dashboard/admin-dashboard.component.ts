import { Component, ChangeDetectionStrategy } from '@angular/core';
import { UserDataService } from '../../../services/user-data.service';

@Component({
  selector: 'app-admin-dashboard',
  standalone: false,
  templateUrl: './admin-dashboard.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './admin-dashboard.component.scss'
})
export class AdminDashboardComponent {
}
