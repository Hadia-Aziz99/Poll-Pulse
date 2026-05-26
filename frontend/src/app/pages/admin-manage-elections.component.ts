import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AdminService } from '../services/admin.service';
import { NgIf, NgFor } from '@angular/common';

@Component({
  selector: 'app-admin-manage-elections',
  standalone: true,
  imports: [RouterLink, NgIf, NgFor],
  templateUrl: './admin-manage-elections.component.html'
})
export class AdminManageElectionsComponent implements OnInit {
  private readonly adminService = inject(AdminService);
  
  elections = signal<any[]>([]);
  isLoading = signal(true);
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);

  ngOnInit(): void {
    this.loadElections();
  }

  loadElections(): void {
    this.isLoading.set(true);
    this.adminService.getElections().subscribe({
      next: (data) => {
        this.elections.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error fetching elections', err);
        this.isLoading.set(false);
      }
    });
  }

  changeStatus(id: string, status: string): void {
    this.errorMessage.set(null);
    this.successMessage.set(null);
    this.adminService.changeElectionStatus(id, status).subscribe({
      next: (res) => {
        this.successMessage.set(res.msg);
        this.loadElections();
      },
      error: (err) => {
        this.errorMessage.set(err.error?.msg || 'Failed to update election status.');
      }
    });
  }

  deleteElection(id: string): void {
    if (!confirm('Are you sure you want to delete this election? This will remove all candidates and cast votes.')) {
      return;
    }

    this.errorMessage.set(null);
    this.successMessage.set(null);
    this.adminService.deleteElection(id).subscribe({
      next: (res) => {
        this.successMessage.set(res.msg);
        this.loadElections();
      },
      error: (err) => {
        this.errorMessage.set(err.error?.msg || 'Failed to delete election.');
      }
    });
  }
}
