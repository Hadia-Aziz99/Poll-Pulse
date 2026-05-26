import { Component, signal } from '@angular/core';
import { NgIf } from '@angular/common';
import { ReactiveFormsModule, FormControl, FormGroup, Validators } from '@angular/forms';

@Component({
  selector: 'app-contact',
  standalone: true,
  imports: [NgIf, ReactiveFormsModule],
  templateUrl: './contact.component.html'
})
export class ContactComponent {
  contactForm = new FormGroup({
    name: new FormControl('', [Validators.required, Validators.minLength(2)]),
    email: new FormControl('', [Validators.required, Validators.email]),
    rating: new FormControl(5, [Validators.required, Validators.min(1), Validators.max(5)]),
    message: new FormControl('', [Validators.required, Validators.minLength(5), Validators.maxLength(1000)])
  });

  submitted = signal(false);

  submit(): void {
    if (this.contactForm.invalid) {
      this.contactForm.markAllAsTouched();
      return;
    }

    this.submitted.set(true);
    this.contactForm.reset({ name: '', email: '', rating: 5, message: '' });
    setTimeout(() => this.submitted.set(false), 5000);
  }
}
