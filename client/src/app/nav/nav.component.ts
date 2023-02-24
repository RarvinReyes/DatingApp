import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { take } from 'rxjs';
import { UserParams } from '../_models/userParams';
import { AccountService } from '../_services/account.service';
import { MembersService } from '../_services/members.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  model: any = {};
  username: string = "";

  constructor(public accountService: AccountService, private router: Router, private toastr: ToastrService,
    private membersService: MembersService) {
  }

  ngOnInit(): void {
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: user => {
        if (user) {
          this.membersService.setUserParams(new UserParams(user));
        }
      }
    });
  }

  login() {
    this.accountService.login(this.model).subscribe({
      next: (res) => {
        this.membersService.setUserParams(new UserParams(res));
        this.router.navigateByUrl('/members');
      },
      error: err => {
        if (err.error) {
          this.toastr.error(err.error);
        } else if (err.length > 0) {
          this.toastr.error(err[0]);
        }
      }
    });
  }

  logout() {
    this.accountService.logout();
    this.router.navigateByUrl('/');
  }
}
