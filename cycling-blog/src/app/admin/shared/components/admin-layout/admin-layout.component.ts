import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-admin-layout',
  templateUrl: './admin-layout.component.html',
  styleUrls: ['./admin-layout.component.scss']
})

export class AdminLayoutComponent {

  constructor(private router: Router) { }

  dropdownMenu(event: Event) {
    event.preventDefault();
    const menu = document.getElementById('main-menu');
    menu!.classList.toggle('is-open');
  };

  logout(event: Event) {
    event.preventDefault();
    this.router.navigate(['/admin', 'login']);
  }
}
