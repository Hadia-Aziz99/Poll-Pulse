import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { AdminService } from '../services/admin.service';
import { ReactiveFormsModule, FormArray, FormBuilder, Validators } from '@angular/forms';
import { NgIf, NgFor, TitleCasePipe } from '@angular/common';

@Component({
  selector: 'app-admin-create-election',
  standalone: true,
  imports: [ReactiveFormsModule, NgIf, NgFor, TitleCasePipe],
  templateUrl: './admin-create-election.component.html'
})
export class AdminCreateElectionComponent {
  private readonly adminService = inject(AdminService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  electionForm = this.fb.group({
    title: ['', [Validators.required, Validators.minLength(5), Validators.maxLength(180)]],
    electionType: ['CR', [Validators.required]],
    degree: ['BSCS', [Validators.required]],
    year: [1, [Validators.required, Validators.min(1), Validators.max(6)]],
    section: ['A', [Validators.required]],
    status: ['draft', [Validators.required]],
    startAt: [''],
    endAt: [''],
    description: [''],
    candidates: this.fb.array([
      this.createCandidateGroup(),
      this.createCandidateGroup(),
      this.createCandidateGroup(),
      this.createCandidateGroup(),
      this.createCandidateGroup()
    ])
  });

  yearsList = [1, 2, 3, 4, 5, 6];
  sectionsList = ['A', 'B', 'C'];
  statusList = ['draft', 'active', 'closed'];

  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);
  isSubmitting = signal(false);

  get candidates(): FormArray {
    return this.electionForm.get('candidates') as FormArray;
  }

  private createCandidateGroup() {
    return this.fb.group({
      name: [''],
      rollNo: ['']
    });
  }

  onSubmit(): void {
    if (this.electionForm.invalid) {
      this.electionForm.markAllAsTouched();
      this.errorMessage.set('Please complete the required election fields correctly.');
      return;
    }

    const raw = this.electionForm.value;
    const validCandidates = (raw.candidates ?? []).filter((c): c is { name: string; rollNo?: string | null } =>
      !!c && typeof c.name === 'string' && c.name.trim().length > 0
    );
    if (validCandidates.length < 2) {
      this.errorMessage.set('Please provide at least two candidate names.');
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    const electionData = {
      title: raw.title,
      electionType: raw.electionType,
      degree: raw.degree,
      year: Number(raw.year),
      section: raw.section,
      status: raw.status,
      startAt: raw.startAt ? new Date(raw.startAt).toISOString() : null,
      endAt: raw.endAt ? new Date(raw.endAt).toISOString() : null,
      description: raw.description,
      candidateNames: validCandidates.map(c => c.name.trim()),
      candidateRollNos: validCandidates.map(c => c.rollNo || '')
    };

    this.adminService.createElection(electionData).subscribe({
      next: (res) => {
        this.isSubmitting.set(false);
        this.successMessage.set(res.msg || 'Election created successfully!');
        setTimeout(() => this.router.navigate(['/admin/elections']), 2000);
      },
      error: (err) => {
        this.isSubmitting.set(false);
        this.errorMessage.set(err.error?.msg || 'Failed to create election.');
      }
    });
  }
}
