import { Component } from '@angular/core';

@Component({
  selector: 'app-main-layout',
  templateUrl: './main-layout.component.html',
  styleUrls: ['./main-layout.component.scss']
})
export class MainLayoutComponent {

  dropdownMenu(event: Event) {
    event.preventDefault();
    const menu = document.getElementById('main-menu');
    menu!.classList.toggle('is-open');
  };
}

