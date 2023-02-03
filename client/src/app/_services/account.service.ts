import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, map } from 'rxjs';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  baseUrl = environment.apiUrl;
  private currentUser = new BehaviorSubject<User | null>(null);
  currentUser$ = this.currentUser.asObservable();

  constructor(private https: HttpClient) { }

  login(model: any) {
    return this.https.post<User>(this.baseUrl + 'accounts/login', model).pipe(
      map((res) => {
        if (res) {
          this.setCurrentUser(res);
        }
      })
    );
  }

  register(model: any) {
    return this.https.post<User>(this.baseUrl + 'accounts/register', model).pipe(
      map((res) => {
        if (res) {
          this.setCurrentUser(res);
        }
      })
    );
  }

  logout() {
    localStorage.removeItem('user');
    this.currentUser.next(null);
  }

  setCurrentUser(user: User | null) {
    localStorage.setItem('user', JSON.stringify(user));
    this.currentUser.next(user);
  }
}
