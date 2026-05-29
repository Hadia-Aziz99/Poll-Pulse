import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { FeedbackService } from '../services/feedback.service';
import { NgIf, NgFor, NgClass, UpperCasePipe, NgSwitch, NgSwitchCase, NgSwitchDefault } from '@angular/common';
import { ReactiveFormsModule, FormControl, FormGroup, Validators } from '@angular/forms';

interface CategoryMeta {
  label: string;
  icon: string;
  description: string;
  helper: string;
  ratings: string[];
}

@Component({
  selector: 'app-feedback',
  standalone: true,
  imports: [RouterLink, NgIf, NgFor, NgClass, ReactiveFormsModule, UpperCasePipe, NgSwitch, NgSwitchCase, NgSwitchDefault],
  templateUrl: './feedback.component.html'
})
export class FeedbackComponent implements OnInit {
  private readonly feedbackService = inject(FeedbackService);
  private readonly route = inject(ActivatedRoute);

  category = 'faculty'; // Updated reactively from route params

  categoryLabels: { [key: string]: string } = {
    faculty: 'Faculty Feedback',
    teacher: 'Teacher Feedback',
    course: 'Course Feedback',
    transport: 'Transport Feedback',
    cafeteria: 'Cafeteria Food Feedback',
    sports: 'Sports Facilities Feedback',
    library: 'Library Feedback',
    event: 'Events Feedback'
  };

  categoryMeta: { [key: string]: CategoryMeta } = {
    faculty: {
      label: 'Faculty Feedback',
      icon: 'fa-building-columns',
      description: 'Administration and department support',
      helper: 'Share views on department administration and CS support.',
      ratings: ['Management', 'Communication', 'Support', 'Responsiveness', 'Overall Experience']
    },
    teacher: {
      label: 'Teacher Feedback',
      icon: 'fa-chalkboard-user',
      description: 'Teaching quality and behavior',
      helper: 'Share views on course delivery, communication, and teaching quality.',
      ratings: ['Teaching Quality', 'Communication', 'Punctuality', 'Course Coverage', 'Behavior']
    },
    course: {
      label: 'Course Feedback',
      icon: 'fa-book-open-reader',
      description: 'Difficulty, content and assessments',
      helper: 'Share views on course contents, assessments and workload.',
      ratings: ['Course Content', 'Difficulty Management', 'Practical Relevance', 'Assessment Fairness', 'Learning Value']
    },
    transport: {
      label: 'Transport Feedback',
      icon: 'fa-bus',
      description: 'Routes, timing and safety',
      helper: 'Share views on bus timings, route safety, and cleanliness.',
      ratings: ['Timing', 'Cleanliness', 'Safety', 'Availability', 'Overall Service']
    },
    cafeteria: {
      label: 'Cafeteria Feedback',
      icon: 'fa-utensils',
      description: 'Food, price and cleanliness',
      helper: 'Share views on cafeteria hygiene, prices, and food options.',
      ratings: ['Food Quality', 'Cleanliness', 'Price Fairness', 'Menu Variety', 'Overall Service']
    },
    sports: {
      label: 'Sports Feedback',
      icon: 'fa-futbol',
      description: 'Facilities and equipment',
      helper: 'Share views on ground maintenance, equipment availability, and sports events.',
      ratings: ['Equipment', 'Ground/Court Quality', 'Availability', 'Management', 'Overall Facilities']
    },
    library: {
      label: 'Library Feedback',
      icon: 'fa-book',
      description: 'Books, seating and timings',
      helper: 'Share views on library seating, silence maintenance, and book availability.',
      ratings: ['Book Availability', 'Study Environment', 'Timing', 'Staff Support', 'Overall Service']
    },
    event: {
      label: 'Event Feedback',
      icon: 'fa-calendar-check',
      description: 'Seminars, trips and activities',
      helper: 'Share views on event management, usefulness, and participation.',
      ratings: ['Organization', 'Management', 'Usefulness', 'Environment', 'Overall Experience']
    }
  };

  teachers = signal<any[]>([]);
  courses = signal<any[]>([]);
  events = signal<any[]>([]);
  recentFeedbacks = signal<any[]>([]);

  feedbackForm = new FormGroup({
    teacher: new FormControl(''),
    course: new FormControl(''),
    event: new FormControl(''),
    targetName: new FormControl(''),
    rating1: new FormControl(5, [Validators.required, Validators.min(1), Validators.max(5)]),
    rating2: new FormControl(5, [Validators.required, Validators.min(1), Validators.max(5)]),
    rating3: new FormControl(5, [Validators.required, Validators.min(1), Validators.max(5)]),
    rating4: new FormControl(5, [Validators.required, Validators.min(1), Validators.max(5)]),
    ratingOverall: new FormControl(5, [Validators.required, Validators.min(1), Validators.max(5)]),
    comment: new FormControl('', [Validators.required, Validators.minLength(5), Validators.maxLength(1200)]),
    suggestion: new FormControl('')
  });

  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);
  isLoading = signal(true);
  isSubmitting = signal(false);


  ngOnInit(): void {
    // Subscribe to route param changes so clicking category buttons actually works
    this.route.paramMap.subscribe(params => {
      const cat = params.get('category');
      if (cat && this.categoryMeta[cat]) {
        this.category = cat;
        this.resetForm();
        this.updateTargetValidators();
      }
    });
    this.loadData();
    this.updateTargetValidators();
  }

  loadData(): void {
    this.isLoading.set(true);
    this.feedbackService.getMasters().subscribe({
      next: (masters) => {
        this.teachers.set(masters.teachers || []);
        this.courses.set(masters.courses || []);
        this.events.set(masters.events || []);
      },
      error: (err) => console.error('Error loading masters', err)
    });

    this.loadRecent();
  }

  loadRecent(): void {
    this.feedbackService.getRecentFeedbacks().subscribe({
      next: (feedbacks) => {
        this.recentFeedbacks.set(feedbacks);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error loading recent feedback', err);
        this.isLoading.set(false);
      }
    });
  }

  resetForm(): void {
    this.feedbackForm.reset({
      teacher: '',
      course: '',
      event: '',
      targetName: '',
      rating1: 5,
      rating2: 5,
      rating3: 5,
      rating4: 5,
      ratingOverall: 5,
      comment: '',
      suggestion: ''
    });
    this.errorMessage.set(null);
    this.successMessage.set(null);
  }

  private updateTargetValidators(): void {
    const teacherControl = this.feedbackForm.controls.teacher;
    const courseControl = this.feedbackForm.controls.course;
    const eventControl = this.feedbackForm.controls.event;
    const targetControl = this.feedbackForm.controls.targetName;

    teacherControl.clearValidators();
    courseControl.clearValidators();
    eventControl.clearValidators();
    targetControl.clearValidators();

    if (this.category === 'teacher') teacherControl.setValidators([Validators.required]);
    else if (this.category === 'course') courseControl.setValidators([Validators.required]);
    else if (this.category === 'event') eventControl.setValidators([Validators.required]);
    else targetControl.setValidators([Validators.required, Validators.minLength(2)]);

    teacherControl.updateValueAndValidity({ emitEvent: false });
    courseControl.updateValueAndValidity({ emitEvent: false });
    eventControl.updateValueAndValidity({ emitEvent: false });
    targetControl.updateValueAndValidity({ emitEvent: false });
  }

  onSubmit(): void {
    this.updateTargetValidators();
    if (this.feedbackForm.invalid) {
      this.feedbackForm.markAllAsTouched();
      this.errorMessage.set('Please complete all required feedback fields.');
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    const raw = this.feedbackForm.value;
    const feedbackData = {
      category: this.category,
      teacher: this.category === 'teacher' ? raw.teacher : null,
      course: this.category === 'course' ? raw.course : null,
      event: this.category === 'event' ? raw.event : null,
      targetName: raw.targetName || null,
      rating1: Number(raw.rating1),
      rating2: Number(raw.rating2),
      rating3: Number(raw.rating3),
      rating4: Number(raw.rating4),
      ratingOverall: Number(raw.ratingOverall),
      comment: raw.comment,
      suggestion: raw.suggestion || null
    };

    this.feedbackService.submitFeedback(feedbackData).subscribe({
      next: (res) => {
        this.isSubmitting.set(false);
        this.successMessage.set(res.msg);
        this.resetForm();
        this.loadRecent();
      },
      error: (err) => {
        this.isSubmitting.set(false);
        this.errorMessage.set(err.error?.msg || 'Failed to submit feedback.');
      }
    });
  }

  get keys(): string[] {
    return Object.keys(this.categoryMeta);
  }
}
