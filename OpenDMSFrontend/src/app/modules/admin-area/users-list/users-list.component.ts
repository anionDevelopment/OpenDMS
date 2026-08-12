import { Component, ChangeDetectionStrategy } from '@angular/core';
import { UserOverviewDTO, UserService } from '../../../generated/open-dms-backend';
import { StorageService } from '../../../services/storage.service';

@Component({
  selector: 'app-users-list',
  standalone: false,
  templateUrl: './users-list.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './users-list.component.scss'
})
export class UsersListComponent {
  users: UserOverviewDTO[] = [];
  allRoles: string[] = [];

  constructor(private storageService: StorageService, private userService: UserService) {
    this.reload();
  }

  private reload(): void {
    const accessToken = this.storageService.getAccessToken();
    this.userService.aPIV3UserControllerGetAllRolesGet(accessToken).subscribe(roles => this.allRoles = (roles ?? []).sort());
    this.userService.aPIV3UserControllerGetAllUsersGet(accessToken).subscribe(users => this.users = users ?? []);
  }

  userHasRole(user: UserOverviewDTO, role: string): boolean {
    return (user.roles ?? new Set<string>()).has(role);
  }

  onRoleToggled(user: UserOverviewDTO, role: string, checked: boolean): void {
    const roles = new Set<string>(user.roles ?? []);
    if (checked) {
      roles.add(role);
    } else {
      roles.delete(role);
    }
    user.roles = roles;
    const accessToken = this.storageService.getAccessToken();
    this.userService.aPIV3UserControllerSetRolesOfUserPut(accessToken, user.id ?? '', Array.from(roles)).subscribe();
  }
}
