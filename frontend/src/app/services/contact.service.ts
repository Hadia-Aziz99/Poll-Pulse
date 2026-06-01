import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface ContactSubmitRequest {
  name: string;
  email: string;
  rating: number;
  message: string;
}

export interface ContactSubmitResponse {
  msg: string;
  id: string;
}

@Injectable({
  providedIn: 'root'
})
export class ContactService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = 'http://localhost:5007/api/contacts';

  submitContactMessage(data: ContactSubmitRequest): Observable<ContactSubmitResponse> {
    return this.http.post<ContactSubmitResponse>(this.apiUrl, data);
  }
}