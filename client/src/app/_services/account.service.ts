import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, map } from 'rxjs';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';
import { PresenceService } from './presence.service';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  baseUrl = environment.apiUrl;
  private currentUser = new BehaviorSubject<User | null>(null);
  currentUser$ = this.currentUser.asObservable();

  constructor(private https: HttpClient, private presenceService: PresenceService) { }

  login(model: any) {
    return this.https.post<User>(this.baseUrl + 'accounts/login', model).pipe(
      map((res) => {
        if (res) {
          this.setCurrentUser(res);
        }
        return res;
      })
    );
  }

  register(model: any) {
    return this.https.post<User>(this.baseUrl + 'accounts/register', model).pipe(
      map((res) => {
        if (res) {
          this.setCurrentUser(res);
        }
        return res;
      })
    );
  }

  logout() {
    localStorage.removeItem('user');
    this.currentUser.next(null);
    this.presenceService.stopHubConnection();
  }

  setCurrentUser(user: User) {
    const roles = this.getDecodedToken(user.token).role;
    user.roles = Array.isArray(roles) ? roles : [roles]; 
    localStorage.setItem('user', JSON.stringify(user));
    this.currentUser.next(user);
    this.presenceService.createHubConnection(user);
  }

  getDecodedToken(token: string) {
    return JSON.parse(atob(token.split('.')[1]));
  }
}
