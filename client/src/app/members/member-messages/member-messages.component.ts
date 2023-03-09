import { Component, Input, OnInit } from '@angular/core';
import { Message } from 'src/app/_models/message';
import { MembersService } from 'src/app/_services/members.service';
import { MessageService } from 'src/app/_services/message.service';

@Component({
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css']
})
export class MemberMessagesComponent implements OnInit {
  @Input() username?: string;
  @Input() messages?: Message[];
  messageContent: string = '';

  constructor(private messageService: MessageService) { }

  ngOnInit(): void {
  }

  SendMessage() {
    if(!this.username) return;
    this.messageService.SendMessage(this.username, this.messageContent).subscribe({
      next: message => {
        this.messages?.push(message);
        this.messageContent = '';
      }
    });
  }

}
