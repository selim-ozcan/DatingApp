import {
  ChangeDetectionStrategy,
  Component,
  Input,
  OnDestroy,
  OnInit,
} from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { take } from 'rxjs';
import { Message } from 'src/app/_models/message';
import { AccountService } from 'src/app/_services/account.service';
import { MessageService } from 'src/app/_services/message.service';
import { environment } from 'src/environments/environment';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css'],
})
export class MemberMessagesComponent implements OnInit {
  @Input('username') username: string | undefined;
  loading = false;
  message: string = '';

  constructor(public messageService: MessageService) {}

  ngOnInit(): void {}

  sendMessage() {
    if (!this.username) return;
    this.loading = true;
    this.messageService
      .sendMessage(this.username, this.message)
      .then(() => {
        this.message = '';
      })
      .finally(() => (this.loading = false));
  }
}
