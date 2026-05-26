import { Component, OnInit, inject, signal, Input } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { ElectionService } from '../services/election.service';
import { NgIf, NgFor } from '@angular/common';
import { ReactiveFormsModule, FormControl, FormGroup, Validators } from '@angular/forms';

@Component({
  selector: 'app-election-detail',
  standalone: true,
  imports: [RouterLink, NgIf, NgFor, ReactiveFormsModule],
  templateUrl: './election-detail.component.html'
})
export class ElectionDetailComponent implements OnInit {
  private readonly electionService = inject(ElectionService);
  private readonly router = inject(Router);

  @Input() id!: string;

  election = signal<any>(null);
  candidates = signal<any[]>([]);
  voteForm = new FormGroup({
    candidateId: new FormControl('', [Validators.required])
  });

  isLoading = signal(true);
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);

  ngOnInit(): void {
    this.loadElectionDetails();
  }

  loadElectionDetails(): void {
    this.electionService.getElectionDetails(this.id).subscribe({
      next: (res) => {
        this.election.set(res.election);
        this.candidates.set(res.candidates);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error fetching election details', err);
        this.errorMessage.set(err.error?.msg || 'Failed to load election details.');
        this.isLoading.set(false);
      }
    });
  }

  onSubmit(): void {
    if (this.voteForm.invalid) {
      this.voteForm.markAllAsTouched();
      this.errorMessage.set('Please select a candidate first.');
      return;
    }

    const candidateId = this.voteForm.value.candidateId ?? '';
    this.errorMessage.set(null);
    this.electionService.castVote(this.id, candidateId).subscribe({
      next: (res) => {
        this.successMessage.set(res.msg || 'Vote cast successfully!');
        if (this.election()) {
          this.election.set({ ...this.election(), hasVoted: true });
        }
      },
      error: (err) => {
        this.errorMessage.set(err.error?.msg || 'Failed to cast vote.');
      }
    });
  }
}
