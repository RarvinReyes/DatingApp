import { Component, OnInit } from '@angular/core';
import { Observable, of } from 'rxjs';
import { User } from '../_models/user';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  model: any = {};
  username: string = "";
 
  constructor(public accountService: AccountService) { }

  ngOnInit(): void {}

  getUsername(): string {
    return this.username = JSON.parse(localStorage.getItem('user') ?? 'null')?.username;
  }

  login() {
    this.accountService.login(this.model).subscribe({
      next: res => { 
        console.log(res); 
      },
      error: err => {
        console.log(err);
      }
    });
  }

  logout() {
    this.accountService.logout();
  }
}
