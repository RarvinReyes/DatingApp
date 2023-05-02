import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { HubConnection } from '@microsoft/signalr';
import { HubConnectionBuilder } from '@microsoft/signalr/dist/esm/HubConnectionBuilder';
import { ToastrService } from 'ngx-toastr';
import { BehaviorSubject, take } from 'rxjs';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class PresenceService {
  hubUrl = environment.hubUrl;
  private hubConnnection?: HubConnection;
  private onlineUserSource = new BehaviorSubject<string[]>([]);
  onlineUserSource$ = this.onlineUserSource.asObservable();

  constructor(private toastr: ToastrService, private router: Router) { }

  createHubConnection(user: User) {
    this.hubConnnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl + "presence", {
        accessTokenFactory: () => { return user.token; }
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnnection.start().catch(error => console.log("error in starting presence hub", error));

    this.hubConnnection.on("UserIsOnline", username => {
      //this.toastr.info(username + " is now online");
    });

    this.hubConnnection.on("UserIsOffline", username => {
      var onlineUsers = this.onlineUserSource.value.filter(f => f != username);
      this.onlineUserSource.next(onlineUsers);
    });

    this.hubConnnection.on("GetOnlineUsers", usernames => {
      this.onlineUserSource.next(usernames);
    });

    this.hubConnnection.on("NewMessageReceived", ({username, knownAs}) => {
      this.toastr.info(knownAs + " has sent a new message! Click me to see it.")
      .onTap
      .pipe(take(1))
      .subscribe({
        next: () => { this.router.navigateByUrl('/members/' + username + '?tab=Messages')}
      });
    });
  }

  stopHubConnection() {
    this.hubConnnection?.stop().catch(error => console.log(error));
  }
}
