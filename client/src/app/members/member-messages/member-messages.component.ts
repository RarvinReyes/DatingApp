import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Message } from 'src/app/_models/message';
import { MembersService } from 'src/app/_services/members.service';
import { MessageService } from 'src/app/_services/message.service';

@Component({
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css']
})
export class MemberMessagesComponent implements OnInit {
  @ViewChild('messageForm') messageForm?: NgForm;
  @Input() username?: string;
  messageContent: string = '';

  constructor(public messageService: MessageService) { }

  ngOnInit(): void {
  }

  SendMessage() {
    if(!this.username) return;
    this.messageService.SendMessage(this.username, this.messageContent).then(() => {
      this.messageForm?.reset();
    });
  }

}
