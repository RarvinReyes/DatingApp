import { Component, OnInit } from '@angular/core';
import { Message } from '../_models/message';
import { Pagination } from '../_models/pagination';
import { MessageService } from '../_services/message.service';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css']
})
export class MessagesComponent implements OnInit {
  messages?: Message[];
  pagination?: Pagination;
  container = 'Unread';
  pageNumber = 1;
  pageSize = 5;
  loading = false;

  constructor(private messageService: MessageService) { }

  ngOnInit(): void {
    if (this.messageService.GetContainer() == '')
      this.messageService.SetContainer(this.container);
    else
      this.container = this.messageService.GetContainer();
    this.loadMessages();
  }

  loadMessages() {
    this.loading = true;
    this.messageService.SetContainer(this.container);
    this.messageService.getMessages(this.pageNumber, this.pageSize)
      .subscribe({
        next: res => {
          this.messages = res.result;
          this.pagination = res.pagination;
          this.loading = false;
        }
      });
  }

  deleteMessage(id: number) {
    this.messageService.DeleteMessage(id).subscribe({
      next: () => {this.messages?.splice(this.messages.findIndex(f => f.id == id), 1)}
    })
  }

  pageChanged(event: any) {
    if (this.pageNumber !== event.page) {
      this.pageNumber = event.page;
      this.loadMessages();
    }
  }
}
