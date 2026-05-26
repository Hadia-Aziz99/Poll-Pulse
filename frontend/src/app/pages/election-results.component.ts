import { Component, OnInit, inject, signal, Input } from '@angular/core';

import { ElectionService } from '../services/election.service';
import { NgIf, NgFor, NgStyle } from '@angular/common';

@Component({
  selector: 'app-election-results',
  standalone: true,
  imports: [NgIf, NgFor, NgStyle],
  templateUrl: './election-results.component.html'
})
export class ElectionResultsComponent implements OnInit {
  private readonly electionService = inject(ElectionService);

  @Input() id!: string;

  resultsData = signal<any>(null);
  isLoading = signal(true);
  errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    this.electionService.getElectionResults(this.id).subscribe({
      next: (res) => {
        this.resultsData.set(res);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error fetching election results', err);
        this.errorMessage.set(err.error?.msg || 'Failed to load election results.');
        this.isLoading.set(false);
      }
    });
  }
}
