import { Component, OnInit, inject, signal } from '@angular/core';
import { AdminService } from '../services/admin.service';
import { NgIf, NgFor } from '@angular/common';

@Component({
  selector: 'app-admin-classes',
  standalone: true,
  imports: [NgIf, NgFor],
  templateUrl: './admin-classes.component.html'
})
export class AdminClassesComponent implements OnInit {
  private readonly adminService = inject(AdminService);
  classes = signal<any[]>([]);
  isLoading = signal(true);

  ngOnInit(): void {
    this.adminService.getClasses().subscribe({
      next: (data) => {
        this.classes.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error fetching classes', err);
        this.isLoading.set(false);
      }
    });
  }
}
