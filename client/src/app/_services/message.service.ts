import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { HttpTransportType } from '@microsoft/signalr';
import { HubConnection } from '@microsoft/signalr/dist/esm/HubConnection';
import { HubConnectionBuilder } from '@microsoft/signalr/dist/esm/HubConnectionBuilder';
import { BehaviorSubject } from 'rxjs';
import { environment } from 'src/environments/environment';
import { Connection, Group } from '../_models/group';
import { Message } from '../_models/message';
import { User } from '../_models/user';
import { getPaginatedResult, getPaginationHeaders } from './paginationHelper';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  baseUrl = environment.apiUrl;
  container = '';
  hubUrl = environment.hubUrl;
  private hubConnnection?: HubConnection;
  private messageThreadSource = new BehaviorSubject<Message[]>([]);
  messageThreadSource$ = this.messageThreadSource.asObservable();

  constructor(private http: HttpClient) { }

  createHubConnection(user: User, otherUser: string) {
    this.hubConnnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl + "message?user=" + otherUser, {
        accessTokenFactory: () => { return user.token; }
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnnection.start().catch(error => console.log("error in starting message hub", error));

    this.hubConnnection.on("ReceiveMessageThread", (messages: Message[]) => {
      this.messageThreadSource.next(messages.reverse());
    });

    this.hubConnnection.on("UpdatedGroup", (group: Group) => {
      console.log("group", group);
      var connections = group.connections;
      console.log("otherUser", otherUser);
      const userOnline = (conn: Connection) => conn.username === otherUser;
      if (group.connections.some(userOnline)) {
        var messageThread = this.messageThreadSource.value;
        console.log("messageThread", messageThread);
        messageThread.forEach(f => {
          if (!f.dateRead) {
            f.dateRead = new Date(Date.now())
          }
        });
        console.log("messageThread2", messageThread);
        this.messageThreadSource.next(messageThread);
      }
      console.log("heloooooooooo")
    });


    this.hubConnnection.on("NewMessage", (message: Message) => {
      var messageThread = this.messageThreadSource.value.reverse();
      messageThread.push(message);
      this.messageThreadSource.next(messageThread.reverse());
    });
  }

  stopHubConnection() {
    if (this.hubConnnection)
      this.hubConnnection.stop().catch(error => console.log(error));
  }

  getMessages(pageNumber: number, pageSize: number) {
    let params = getPaginationHeaders(pageNumber, pageSize);
    params = params.append('Container', this.container);
    return getPaginatedResult<Message[]>(this.baseUrl + 'messages', params, this.http);
  }

  GetContainer() {
    return this.container;
  }

  SetContainer(container: string) {
    this.container = container;
  }

  GetMessageThread(username: string) {
    return this.http.get<Message[]>(this.baseUrl + 'messages/thread/' + username);
  }

  async SendMessage(username: string, content: string) {
    return this.hubConnnection?.invoke("SendMessage", { recipientUsername: username, content })
      .catch(error => console.log("SendMessageErr", error));
  }

  DeleteMessage(id: number) {
    return this.http.delete(this.baseUrl + 'messages/' + id);
  }
}
