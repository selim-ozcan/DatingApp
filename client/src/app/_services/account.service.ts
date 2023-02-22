import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, map, Observable } from 'rxjs';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root',
})
export class AccountService {
  baseUrl = 'https://localhost:5001/api/';
  private currentUserSource: BehaviorSubject<User | null> =
    new BehaviorSubject<User | null>(null);
  currentUser$ = this.currentUserSource.asObservable();

  constructor(private http: HttpClient) {
    // App çalıştırıldığında. Yani browser kapatıp açıldığında vs. log in olmuş bir kullanıcı var mı diye kontrol etmek. Kursta bu olayı AppComponent'in ngOnInit metodunda yapıyor.
    if (localStorage.length > 0) {
      const userString = localStorage.getItem('user');
      if (!userString) return;
      const user: User = JSON.parse(userString);
      this.currentUserSource.next(user);
    }
  }

  login(model: any) {
    return this.http.post<User>(this.baseUrl + 'account/login', model).pipe(
      map((response: User) => {
        const user = response;
        if (user) localStorage.setItem('user', JSON.stringify(user));
        this.currentUserSource.next(user);

        return user;
      })
    );
  }

  register(model: any) {
    return this.http.post<User>(this.baseUrl + 'account/register', model).pipe(
      map((response: User) => {
        const user = response;
        if (user) localStorage.setItem('user', JSON.stringify(user));
        this.currentUserSource.next(user);

        return user;
      })
    );
  }

  setCurrentUser(user: User) {
    this.currentUserSource.next(user);
  }

  logout() {
    localStorage.removeItem('user');
    this.currentUserSource.next(null);
  }
}
