import { Component } from '@angular/core';

@Component({
  selector: 'app-navbar-items',
  templateUrl: './navbar-items.component.html',
  styleUrls: ['./navbar-items.component.scss']
})
export class NavbarItemsComponent {

  dropdownMenu(event: Event) {
    event.preventDefault();
    const menu = document.getElementById('main-menu');
    menu!.classList.toggle('is-open');
  };
}
