import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ElectionService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = 'http://localhost:5007/api/elections';

  getDashboard(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/dashboard`);
  }

  listElections(): Observable<any[]> {
    return this.http.get<any[]>(this.apiUrl);
  }

  getElectionDetails(id: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${id}`);
  }

  castVote(id: string, candidateId: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/${id}/vote`, { candidateId });
  }

  getElectionResults(id: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${id}/results`);
  }
}
