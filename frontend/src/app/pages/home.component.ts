import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { NgStyle } from '@angular/common';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [RouterLink, NgStyle],
  templateUrl: './home.component.html'
})
export class HomeComponent implements OnInit {
  private readonly http = inject(HttpClient);
  metrics = signal<any>({
    activeElections: 0,
    closedElections: 0,
    totalFeedback: 0,
    sentiment: { positive: 0, neutral: 0, negative: 0 }
  });

  ngOnInit(): void {
    this.http.get<any>('http://localhost:5007/api/public/metrics').subscribe({
      next: (res) => this.metrics.set(res),
      error: (err) => console.error('Error loading home metrics', err)
    });
  }
}
