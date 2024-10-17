import { Component } from '@angular/core';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrl: './nav.component.scss'
})
export class NavComponent {
  mainMenu = ["Main", "Recipies", "About", "Contact"];
  isMenuOpen = false;
  
  toggleMenu(): void {
    this.isMenuOpen = !this.isMenuOpen
  }
}