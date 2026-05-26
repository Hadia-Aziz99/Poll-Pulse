import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = 'http://localhost:5007/api/admin';

  getDashboard(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/dashboard`);
  }

  getClasses(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/classes`);
  }

  getElections(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/elections`);
  }

  createElection(electionData: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/elections`, electionData);
  }

  changeElectionStatus(id: string, status: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/elections/${id}/status/${status}`, {});
  }

  deleteElection(id: string): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/elections/${id}`);
  }

  getMasters(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/masters`);
  }

  createTeacher(teacherData: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/masters/teachers`, teacherData);
  }

  createCourse(courseData: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/masters/courses`, courseData);
  }

  createEvent(eventData: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/masters/events`, eventData);
  }

  getFeedbackList(category?: string, sentiment?: string): Observable<any[]> {
    let params = new HttpParams();
    if (category) params = params.set('category', category);
    if (sentiment) params = params.set('sentiment', sentiment);
    return this.http.get<any[]>(`${this.apiUrl}/feedback`, { params });
  }

  getSentimentMetrics(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/sentiment`);
  }

  getAuditLogs(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/audit-logs`);
  }
}
