import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Message } from '../_models/message';
import { getPaginatedResult, getPaginationHeaders } from './paginationHelper';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  baseUrl = environment.apiUrl;
  container = '';

  constructor(private http: HttpClient) { }

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

  SendMessage(username: string, content: string) {
    return this.http.post<Message>(this.baseUrl + 'messages', { recipientUsername: username, content });
  }

  DeleteMessage(id: number) {
    return this.http.delete(this.baseUrl + 'messages/' + id);
  }
}
