import { Component, OnInit, inject, signal } from '@angular/core';

import { AdminService } from '../services/admin.service';
import { NgIf, NgFor, NgClass, DatePipe } from '@angular/common';
import { ReactiveFormsModule, FormControl, FormGroup } from '@angular/forms';

@Component({
  selector: 'app-admin-feedback',
  standalone: true,
  imports: [NgIf, NgFor, NgClass, DatePipe, ReactiveFormsModule],
  templateUrl: './admin-feedback.component.html'
})
export class AdminFeedbackComponent implements OnInit {
  private readonly adminService = inject(AdminService);

  feedbacks = signal<any[]>([]);
  isLoading = signal(true);

  filterForm = new FormGroup({
    category: new FormControl(''),
    sentiment: new FormControl('')
  });

  categories = [
    { value: 'faculty', label: 'Faculty Feedback' },
    { value: 'teacher', label: 'Teacher Feedback' },
    { value: 'course', label: 'Course Feedback' },
    { value: 'transport', label: 'Transport Feedback' },
    { value: 'cafeteria', label: 'Cafeteria Food' },
    { value: 'sports', label: 'Sports Facilities' },
    { value: 'library', label: 'Library' },
    { value: 'event', label: 'Events' }
  ];

  sentiments = ['Positive', 'Neutral', 'Negative'];

  ngOnInit(): void {
    this.loadFeedback();
  }

  loadFeedback(): void {
    this.isLoading.set(true);
    const category = this.filterForm.value.category || undefined;
    const sentiment = this.filterForm.value.sentiment || undefined;
    this.adminService.getFeedbackList(category, sentiment).subscribe({
      next: (data) => {
        this.feedbacks.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error fetching feedback list', err);
        this.isLoading.set(false);
      }
    });
  }

  applyFilters(): void {
    this.loadFeedback();
  }

  resetFilters(): void {
    this.filterForm.reset({ category: '', sentiment: '' });
    this.loadFeedback();
  }

  getSentimentClass(label: string): string {
    if (label === 'Positive') return 'bg-success-subtle text-success border border-success-subtle';
    if (label === 'Negative') return 'bg-danger-subtle text-danger border border-danger-subtle';
    return 'bg-warning-subtle text-warning border border-warning-subtle';
  }
}
