import { Component } from '@angular/core';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.scss']
})
export class NavComponent {

  dropdownMenuOpen(event: Event) {
    event.preventDefault();
    const menu = document.getElementById('main-menu');
    menu!.classList.toggle('is-open');
  }

  dropdownMenuClose(event: Event) {
    event.preventDefault();
    const menu = document.getElementById('main-menu');
    menu!.classList.remove('is-open');
  }
}
