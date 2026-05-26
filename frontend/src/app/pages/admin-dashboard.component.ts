import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AdminService } from '../services/admin.service';
import { NgIf, NgFor, NgStyle, DatePipe } from '@angular/common';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [RouterLink, NgIf, NgFor, NgStyle, DatePipe],
  templateUrl: './admin-dashboard.component.html'
})
export class AdminDashboardComponent implements OnInit {
  private readonly adminService = inject(AdminService);

  dashboardData = signal<any>({
    metrics: {
      totalStudents: 0,
      activeElections: 0,
      closedElections: 0,
      totalVotes: 0,
      totalFeedback: 0,
      sentiment: { positive: 0, neutral: 0, negative: 0 }
    },
    recentElections: [],
    recentLogs: []
  });

  isLoading = signal(true);

  ngOnInit(): void {
    this.adminService.getDashboard().subscribe({
      next: (data) => {
        this.dashboardData.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error fetching admin dashboard', err);
        this.isLoading.set(false);
      }
    });
  }
}
