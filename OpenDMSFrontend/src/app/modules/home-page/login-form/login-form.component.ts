import { Component } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { UserService } from '../../../generated/open-dms-backend';
import { Router } from '@angular/router';
import { UserDataService } from '../../../services/user-data.service';
import { StorageService } from '../../../services/storage.service';

@Component({
  selector: 'app-login-form',
  standalone: false,
  templateUrl: './login-form.component.html',
  styleUrl: './login-form.component.scss'
})
export class LoginFormComponent {

  constructor(private userService: UserService, private userDataService: UserDataService, private router: Router, private storageService: StorageService) {
  }
  form: FormGroup = new FormGroup({
    username: new FormControl(''),
    password: new FormControl(''),
  });

  public login(): void {
    const username: string = this.form.get('username')!.value;
    const password: string = this.form.get('password')!.value;
    this.userService.aPIV2UserControllerLoginPut(username, password).subscribe((a: any) => {
      this.storageService.setAccessToken(a.value!);
      this.userDataService.loadUserData().subscribe(() => {
        this.router.navigate(['user', 'dashboard']);
      });
    },
      (error: any) => {
        console.error(error);
      });
  }
}
