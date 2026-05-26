import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { ElectionService } from '../services/election.service';
import { NgIf, NgFor } from '@angular/common';

@Component({
  selector: 'app-elections',
  standalone: true,
  imports: [RouterLink, NgIf, NgFor],
  templateUrl: './elections.component.html'
})
export class ElectionsComponent implements OnInit {
  protected readonly authService = inject(AuthService);
  private readonly electionService = inject(ElectionService);

  elections = signal<any[]>([]);
  isLoading = signal(true);

  ngOnInit(): void {
    this.electionService.listElections().subscribe({
      next: (data) => {
        this.elections.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error fetching elections list', err);
        this.isLoading.set(false);
      }
    });
  }
}
