import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { ReactiveFormsModule, FormGroup, FormControl, Validators } from '@angular/forms';
import { NgIf, NgFor } from '@angular/common';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [RouterLink, ReactiveFormsModule, NgIf, NgFor],
  templateUrl: './register.component.html'
})
export class RegisterComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  registerForm = new FormGroup({
    name: new FormControl('', [Validators.required, Validators.minLength(2)]),
    email: new FormControl('', [
      Validators.required, 
      Validators.email, 
      Validators.pattern(/^\d{7}@students\.au\.edu\.pk$/i)
    ]),
    degree: new FormControl('', [Validators.required]),
    year: new FormControl<number | null>(null, [Validators.required]),
    section: new FormControl('', [Validators.required]),
    password: new FormControl('', [Validators.required, Validators.minLength(6)])
  });

  yearsList = [1, 2, 3, 4, 5, 6];
  sectionsList = ['A', 'B', 'C'];

  errorMessage = signal<string | null>(null);
  isLoading = signal(false);

  onSubmit(): void {
    if (this.registerForm.invalid) {
      this.errorMessage.set('Please fill out the form correctly.');
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    const registerData = {
      name: this.registerForm.value.name ?? '',
      email: this.registerForm.value.email ?? '',
      degree: this.registerForm.value.degree ?? '',
      year: Number(this.registerForm.value.year),
      section: this.registerForm.value.section ?? '',
      password: this.registerForm.value.password ?? ''
    };

    this.authService.register(registerData).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.isLoading.set(false);
        this.errorMessage.set(err.error?.msg || 'Failed to register account.');
      }
    });
  }
}
