import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, map } from 'rxjs';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class AccountService {

  baseUrl = 'https://localhost:5001/api/';
  private currentUser = new BehaviorSubject<User | null>(null);
  currentUser$ = this.currentUser.asObservable();

  constructor(private https: HttpClient) { }

  login(model: any) {
    return this.https.post<User>(this.baseUrl + 'accounts/login', model).pipe(
      map((res) => {
        if (res) {
          localStorage.setItem('user', JSON.stringify(res));
          this.currentUser.next(res);
        }
      })
    );
  }

  register(model: any) {
    return this.https.post<User>(this.baseUrl + 'accounts/register', model).pipe(
      map((res) => {
        if (res) {
          localStorage.setItem('user', JSON.stringify(res));
          this.currentUser.next(res);
        }
      })
    );
  }

  logout() {
    localStorage.removeItem('user');
    this.currentUser.next(null);
  }

  setCurrentUser(user: User | null) {
    this.currentUser.next(user);
  }
}
