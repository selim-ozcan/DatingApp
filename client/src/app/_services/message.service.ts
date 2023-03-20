import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { ToastrService } from 'ngx-toastr';
import { BehaviorSubject, take } from 'rxjs';
import { environment } from 'src/environments/environment';
import { Message } from '../_models/message';
import { User } from '../_models/user';
import { getPaginatedResult, getPaginationHeaders } from './paginationHelper';

@Injectable({
  providedIn: 'root',
})
export class MessageService {
  hubUrl: string = environment.hubUrl;
  hubConnection: HubConnection | undefined;

  private messageThreadSource = new BehaviorSubject<Message[]>([]);
  messageThread$ = this.messageThreadSource.asObservable();

  baseUrl: string = environment.baseUrl;

  constructor(private http: HttpClient) {}

  createHubConnection(user: User, otherUsername: string) {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl + 'message?user=' + otherUsername, {
        accessTokenFactory: () => user.token,
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start().catch((error) => console.log(error));

    this.hubConnection?.on('ReceiveMessageThread', (messages) => {
      this.messageThreadSource.next(messages);
    });

    this.hubConnection?.on('NewMessage', (message) => {
      this.messageThread$.pipe(take(1)).subscribe({
        next: (thread) => {
          this.messageThreadSource.next([...thread, message]);
        },
      });
    });
  }

  stopHubConnection() {
    this.hubConnection?.stop();
  }

  async sendMessage(username: string, content: string) {
    if (!this.hubConnection) return Promise.resolve();
    return this.hubConnection
      .invoke('SendMessage', {
        RecipientUsername: username,
        Content: content,
      })
      .catch((error) => console.log(error));
  }

  getMessages(pageNumber: number, pageSize: number, container: string) {
    let params = getPaginationHeaders(pageNumber, pageSize);
    params = params.append('Container', container);

    return getPaginatedResult<Message[]>(
      `${this.baseUrl}messages`,
      params,
      this.http
    );
  }

  deleteMessage(id: number) {
    return this.http.delete(`${this.baseUrl}messages/${id}`);
  }
}
