import { Component, OnInit } from '@angular/core';
import { User } from './_models/user';
import { AccountService } from './_services/account.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {

  constructor(private accountService: AccountService) {}

  title = 'Dating App';
  users: any;

  ngOnInit(): void {
    this.setCurrentUser();
  }


  setCurrentUser() {
    const user: User | null = JSON.parse(localStorage.getItem('user') ?? 'null');
    this.accountService.setCurrentUser(user)
  }
}
