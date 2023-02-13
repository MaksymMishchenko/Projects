import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';

import { NavbarItemsComponent } from './navbar-items.component';

describe('NavbarItemsComponent', () => {
  let component: NavbarItemsComponent;
  let fixture: ComponentFixture<NavbarItemsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ NavbarItemsComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(NavbarItemsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create navbar items', () => {
    expect(component).toBeTruthy();
    const homeEl = fixture.debugElement.query(By.css('home'));
    expect(homeEl.nativeElement.textContent).toEqual('Home');

    const bikesEl = fixture.debugElement.query(By.css('bikes'));
    expect(bikesEl.nativeElement.textContent).toEqual('Bikes');

    const whereEl = fixture.debugElement.query(By.css('where'));
    expect(whereEl.nativeElement.textContent).toEqual('Where to ride');

    const aboutEl = fixture.debugElement.query(By.css('about'));
    expect(aboutEl.nativeElement.textContent).toEqual('About us');
  });
});
