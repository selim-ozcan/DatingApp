import { Component, Input, OnInit } from '@angular/core';
import { Message } from 'src/app/_models/message';
import { MessageService } from 'src/app/_services/message.service';

@Component({
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css'],
})
export class MemberMessagesComponent implements OnInit {
  @Input('messages') messages: Message[] = [];
  @Input('username') username: string | undefined;
  message: string = '';

  constructor(private messageService: MessageService) {}

  ngOnInit(): void {}

  sendMessage() {
    if (!this.username) return;
    this.messageService.sendMessage(this.username, this.message).subscribe({
      next: (msg) => {
        this.messages.push(msg);
        this.message = '';
      },
    });
  }
}
