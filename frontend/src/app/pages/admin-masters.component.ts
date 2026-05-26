import { Component, OnInit, inject, signal } from '@angular/core';

import { AdminService } from '../services/admin.service';
import { NgIf, NgFor, NgClass, DatePipe, UpperCasePipe } from '@angular/common';
import { ReactiveFormsModule, FormControl, FormGroup, Validators } from '@angular/forms';

@Component({
  selector: 'app-admin-masters',
  standalone: true,
  imports: [NgIf, NgFor, NgClass, DatePipe, UpperCasePipe, ReactiveFormsModule],
  templateUrl: './admin-masters.component.html'
})
export class AdminMastersComponent implements OnInit {
  private readonly adminService = inject(AdminService);

  teachers = signal<any[]>([]);
  courses = signal<any[]>([]);
  events = signal<any[]>([]);
  isLoading = signal(true);
  
  activeTab = 'teachers'; // teachers, courses, events

  teacherForm = new FormGroup({
    name: new FormControl('', [Validators.required, Validators.minLength(2)]),
    email: new FormControl('', [Validators.email])
  });

  courseForm = new FormGroup({
    code: new FormControl('', [Validators.required, Validators.minLength(2)]),
    title: new FormControl('', [Validators.required, Validators.minLength(3)]),
    degree: new FormControl('BSCS', [Validators.required]),
    year: new FormControl(1, [Validators.required, Validators.min(1), Validators.max(6)])
  });

  eventForm = new FormGroup({
    name: new FormControl('', [Validators.required, Validators.minLength(2)]),
    eventDate: new FormControl(''),
    description: new FormControl(''),
    status: new FormControl('completed', [Validators.required])
  });

  yearsList = [1, 2, 3, 4, 5, 6];

  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);
  isSubmitting = signal(false);

  showForm = false; // toggle add form inline

  ngOnInit(): void {
    this.loadMasters();
  }

  loadMasters(): void {
    this.isLoading.set(true);
    this.adminService.getMasters().subscribe({
      next: (data) => {
        this.teachers.set(data.teachers || []);
        this.courses.set(data.courses || []);
        this.events.set(data.events || []);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error fetching masters', err);
        this.isLoading.set(false);
      }
    });
  }

  setTab(tab: string): void {
    this.activeTab = tab;
    this.showForm = false;
    this.errorMessage.set(null);
    this.successMessage.set(null);
  }

  toggleForm(): void {
    this.showForm = !this.showForm;
    this.errorMessage.set(null);
    this.successMessage.set(null);
  }

  submitTeacher(): void {
    if (this.teacherForm.invalid) {
      this.teacherForm.markAllAsTouched();
      this.errorMessage.set('Teacher name is required.');
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    this.adminService.createTeacher(this.teacherForm.value).subscribe({
      next: (res) => {
        this.isSubmitting.set(false);
        this.successMessage.set(res.msg || 'Teacher added successfully!');
        this.teacherForm.reset({ name: '', email: '' });
        this.showForm = false;
        this.loadMasters();
      },
      error: (err) => {
        this.isSubmitting.set(false);
        this.errorMessage.set(err.error?.msg || 'Failed to add teacher.');
      }
    });
  }

  submitCourse(): void {
    if (this.courseForm.invalid) {
      this.courseForm.markAllAsTouched();
      this.errorMessage.set('Course code and title are required.');
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    const raw = this.courseForm.value;
    this.adminService.createCourse({
      code: raw.code,
      title: raw.title,
      degree: raw.degree,
      year: Number(raw.year)
    }).subscribe({
      next: (res) => {
        this.isSubmitting.set(false);
        this.successMessage.set(res.msg || 'Course added successfully!');
        this.courseForm.reset({ code: '', title: '', degree: 'BSCS', year: 1 });
        this.showForm = false;
        this.loadMasters();
      },
      error: (err) => {
        this.isSubmitting.set(false);
        this.errorMessage.set(err.error?.msg || 'Failed to add course.');
      }
    });
  }

  submitEvent(): void {
    if (this.eventForm.invalid) {
      this.eventForm.markAllAsTouched();
      this.errorMessage.set('Event name is required.');
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    const raw = this.eventForm.value;
    this.adminService.createEvent({
      name: raw.name,
      eventDate: raw.eventDate ? new Date(raw.eventDate).toISOString() : null,
      description: raw.description,
      status: raw.status
    }).subscribe({
      next: (res) => {
        this.isSubmitting.set(false);
        this.successMessage.set(res.msg || 'Event added successfully!');
        this.eventForm.reset({ name: '', eventDate: '', description: '', status: 'completed' });
        this.showForm = false;
        this.loadMasters();
      },
      error: (err) => {
        this.isSubmitting.set(false);
        this.errorMessage.set(err.error?.msg || 'Failed to add event.');
      }
    });
  }
}
