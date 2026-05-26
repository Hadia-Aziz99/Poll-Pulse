import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

export interface User {
  id: string;
  name: string;
  email: string;
  rollNo?: string;
  degree?: string;
  year?: number;
  section?: string;
  classKey?: string;
  role: string;
}

export interface AuthResponse {
  token: string;
  user: User;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = 'http://localhost:5007/api/auth';

  // Signals for reactive state
  currentUser = signal<User | null>(null);
  token = signal<string | null>(null);

  constructor() {
    this.loadSession();
  }

  private loadSession(): void {
    if (typeof window !== 'undefined') {
      const savedToken = localStorage.getItem('au_token');
      const savedUser = localStorage.getItem('au_user');
      if (savedToken && savedUser) {
        this.token.set(savedToken);
        this.currentUser.set(JSON.parse(savedUser));
      }
    }
  }

  register(userData: any): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, userData).pipe(
      tap(res => this.handleAuthSuccess(res))
    );
  }

  login(credentials: any): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, credentials).pipe(
      tap(res => this.handleAuthSuccess(res))
    );
  }

  adminLogin(credentials: any): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/admin/login`, credentials).pipe(
      tap(res => this.handleAuthSuccess(res))
    );
  }

  private handleAuthSuccess(res: AuthResponse): void {
    if (typeof window !== 'undefined') {
      localStorage.setItem('au_token', res.token);
      localStorage.setItem('au_user', JSON.stringify(res.user));
    }
    this.token.set(res.token);
    this.currentUser.set(res.user);
  }

  logout(): void {
    if (typeof window !== 'undefined') {
      localStorage.removeItem('au_token');
      localStorage.removeItem('au_user');
    }
    this.token.set(null);
    this.currentUser.set(null);
  }

  isLoggedIn(): boolean {
    return !!this.token();
  }

  isAdmin(): boolean {
    return this.currentUser()?.role === 'admin';
  }
}
