import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { ToastrService } from 'ngx-toastr';
import { BehaviorSubject, take } from 'rxjs';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root',
})
export class PresenceService {
  hubUrl = environment.hubUrl;
  private hubConnection: HubConnection | undefined;
  private onlineUsersSource = new BehaviorSubject<string[]>([]);
  onlineUsers$ = this.onlineUsersSource.asObservable();

  constructor(private toastr: ToastrService, private router: Router) {}

  createHubConnection(user: User) {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl + 'presence', {
        accessTokenFactory: () => user.token,
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on('UserIsOnline', (username) => {
      this.toastr.info(username + ' has connected');
    });

    this.hubConnection.on('UserIsOffline', (username) => {});

    this.hubConnection.on('GetOnlineUsers', (onlineUsers) => {
      this.onlineUsersSource.next(onlineUsers);
    });

    this.hubConnection?.on('NewMessageReceived', (username: string) => {
      console.log(username);
      this.toastr
        .info(
          'You have a new message from ' +
            username.charAt(0).toUpperCase() +
            username.slice(1)
        )
        .onTap.pipe(take(1))
        .subscribe({
          next: () => this.router.navigateByUrl('/messages'),
        });
    });

    this.hubConnection.start().catch((error) => console.log(error));
  }

  stopHubConnection() {
    this.hubConnection?.stop().catch((error) => console.log(error));
  }
}
