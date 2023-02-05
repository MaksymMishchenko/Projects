import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-admin-layout',
  templateUrl: './admin-layout.component.html',
  styleUrls: ['./admin-layout.component.scss']
})

export class AdminLayoutComponent {

  constructor(private router: Router, public auth: AuthService) { }

  dropdownMenu(event: Event) {
    event.preventDefault();
    const menu = document.getElementById('main-menu');
    menu!.classList.toggle('is-open');
  };

  logout(event: Event) {
    event.preventDefault();
    this.auth.logout();
    this.router.navigate(['/admin', 'login']);
  }
}
