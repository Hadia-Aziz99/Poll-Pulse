import { Component, OnInit, inject, signal } from '@angular/core';

import { AdminService } from '../services/admin.service';
import { NgIf, NgFor, DatePipe, UpperCasePipe, NgClass } from '@angular/common';

@Component({
  selector: 'app-admin-audit-logs',
  standalone: true,
  imports: [NgIf, NgFor, DatePipe, UpperCasePipe, NgClass],
  templateUrl: './admin-audit-logs.component.html'
})
export class AdminAuditLogsComponent implements OnInit {
  private readonly adminService = inject(AdminService);

  logs = signal<any[]>([]);
  isLoading = signal(true);

  ngOnInit(): void {
    this.loadAuditLogs();
  }

  loadAuditLogs(): void {
    this.isLoading.set(true);
    this.adminService.getAuditLogs().subscribe({
      next: (data) => {
        this.logs.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error fetching audit logs', err);
        this.isLoading.set(false);
      }
    });
  }

  getBadgeClass(action: string): string {
    const act = action.toUpperCase();
    if (act.includes('DELETE')) return 'bg-danger-subtle text-danger border border-danger-subtle';
    if (act.includes('CREATE') || act.includes('REGISTER')) return 'bg-success-subtle text-success border border-success-subtle';
    if (act.includes('CHANGE') || act.includes('UPDATE')) return 'bg-warning-subtle text-warning border border-warning-subtle';
    if (act.includes('LOGIN')) return 'bg-info-subtle text-info border border-info-subtle';
    return 'bg-secondary-subtle text-dark border border-secondary-subtle';
  }
}
