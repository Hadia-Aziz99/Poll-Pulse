import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class FeedbackService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = 'http://localhost:5007/api/feedbacks';

  getMasters(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/masters`);
  }

  getRecentFeedbacks(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/recent`);
  }

  submitFeedback(feedbackData: any): Observable<any> {
    return this.http.post<any>(this.apiUrl, feedbackData);
  }
}
