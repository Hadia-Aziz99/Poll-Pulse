import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { ElectionService } from '../services/election.service';
import { NgIf, NgFor, UpperCasePipe, DatePipe } from '@angular/common';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterLink, NgIf, NgFor, UpperCasePipe, DatePipe],
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent implements OnInit {
  protected readonly authService = inject(AuthService);
  private readonly electionService = inject(ElectionService);

  dashboardData = signal<any>({
    metrics: {
      openElectionsCount: 0,
      closedElectionsCount: 0,
      votesCast: 0,
      feedbacksSubmitted: 0,
      classSentiment: { positive: 0, neutral: 0, negative: 0 }
    },
    openElections: [],
    closedElections: [],
    recentFeedbacks: []
  });

  isLoading = signal(true);

  ngOnInit(): void {
    this.electionService.getDashboard().subscribe({
      next: (data) => {
        this.dashboardData.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error fetching dashboard', err);
        this.isLoading.set(false);
      }
    });
  }
}
