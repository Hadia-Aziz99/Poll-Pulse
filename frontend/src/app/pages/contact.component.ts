import { Component, inject, signal } from '@angular/core';
import { NgIf } from '@angular/common';
import { ReactiveFormsModule, FormControl, FormGroup, Validators } from '@angular/forms';
import { ContactService, ContactSubmitRequest } from '../services/contact.service';

@Component({
  selector: 'app-contact',
  standalone: true,
  imports: [NgIf, ReactiveFormsModule],
  templateUrl: './contact.component.html'
})
export class ContactComponent {
  private readonly contactService = inject(ContactService);

  contactForm = new FormGroup({
    name: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.minLength(2)] }),
    email: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.email] }),
    rating: new FormControl(5, { nonNullable: true, validators: [Validators.required, Validators.min(1), Validators.max(5)] }),
    message: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.minLength(5), Validators.maxLength(1000)] })
  });

  submitted = signal(false);
  isSubmitting = signal(false);
  errorMessage = signal<string | null>(null);

  submit(): void {
    if (this.contactForm.invalid) {
      this.contactForm.markAllAsTouched();
      return;
    }

    const payload: ContactSubmitRequest = {
      name: this.contactForm.controls.name.value.trim(),
      email: this.contactForm.controls.email.value.trim(),
      rating: Number(this.contactForm.controls.rating.value),
      message: this.contactForm.controls.message.value.trim()
    };

    this.isSubmitting.set(true);
    this.submitted.set(false);
    this.errorMessage.set(null);

    this.contactService.submitContactMessage(payload).subscribe({
      next: () => {
        this.submitted.set(true);
        this.isSubmitting.set(false);
        this.contactForm.reset({ name: '', email: '', rating: 5, message: '' });
        setTimeout(() => this.submitted.set(false), 5000);
      },
      error: (err) => {
        this.errorMessage.set(err.error?.msg || 'Unable to submit your message. Please try again.');
        this.isSubmitting.set(false);
      }
    });
  }
}