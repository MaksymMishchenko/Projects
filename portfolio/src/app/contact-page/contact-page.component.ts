import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';

@Component({
  selector: 'app-contact-page',
  templateUrl: './contact-page.component.html',
  styleUrls: ['./contact-page.component.scss']
})
export class ContactPageComponent implements OnInit {

  message = '';
  contactForm!: FormGroup
  constructor(private fb: FormBuilder) { }

  ngOnInit(): void {
    this.contactForm = this.fb.group({
      name: new FormControl('', [Validators.required]),
      email: new FormControl('', [Validators.required, Validators.email]),
      phone: new FormControl('', [Validators.required]),
      comment: new FormControl('', Validators.required)
    })
  }

  onSubmit() {
    if (this.contactForm.invalid) {
      return;
    }
    this.message = "Дані успішно надіслані. Очікуйте на дзвінок від менеджера"
    this.contactForm.reset();
  }
}
