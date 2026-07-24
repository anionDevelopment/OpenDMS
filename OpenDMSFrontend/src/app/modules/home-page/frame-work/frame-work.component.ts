import { Component } from '@angular/core';
import { Settings } from '../../../static/Settings';
import { Router } from '@angular/router';

@Component({
  selector: 'app-frame-work',
  standalone: false,
  templateUrl: './frame-work.component.html',
  styleUrl: './frame-work.component.scss'
})
export class FrameWorkComponent {
  title: string;
  constructor(private router: Router) {
    this.title = Settings.getAppName();
  }
  onHeaderClick() {
    this.router.navigate(['']);
  }

}
