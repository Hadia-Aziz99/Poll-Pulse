import { Component, OnInit, inject, signal } from '@angular/core';

import { AdminService } from '../services/admin.service';
import { NgIf, NgFor, NgStyle } from '@angular/common';

@Component({
  selector: 'app-admin-sentiment',
  standalone: true,
  imports: [NgIf, NgFor, NgStyle],
  templateUrl: './admin-sentiment.component.html'
})
export class AdminSentimentComponent implements OnInit {
  private readonly adminService = inject(AdminService);

  sentimentData = signal<any>({
    summary: { total: 0, positive: 0, neutral: 0, negative: 0 },
    byCategory: [],
    byClass: []
  });

  isLoading = signal(true);

  ngOnInit(): void {
    this.loadSentimentMetrics();
  }

  loadSentimentMetrics(): void {
    this.isLoading.set(true);
    this.adminService.getSentimentMetrics().subscribe({
      next: (data) => {
        this.sentimentData.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error fetching sentiment metrics', err);
        this.isLoading.set(false);
      }
    });
  }

  getCategoryLabel(cat: string): string {
    const labels: { [key: string]: string } = {
      faculty: 'Faculty Feedback',
      teacher: 'Teacher Feedback',
      course: 'Course Feedback',
      transport: 'Transport Feedback',
      cafeteria: 'Cafeteria Food',
      sports: 'Sports Facilities',
      library: 'Library',
      event: 'Events'
    };
    return labels[cat] || cat;
  }
}
