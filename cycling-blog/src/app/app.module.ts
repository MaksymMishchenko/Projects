import { NgModule, Provider, isDevMode } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HTTP_INTERCEPTORS } from '@angular/common/http';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { MainLayoutComponent } from './shared/components/main-layout/main-layout.component';
import { HomePageComponent } from './home-page/home-page.component';
import { PostPageComponent } from './post-page/post-page.component';
import { BikesPageComponent } from './bikes-page/bikes-page.component';
import { AboutPageComponent } from './about-page/about-page.component';
import { RidePageComponent } from './ride-page/ride-page.component';
import { SharedModule } from './shared/shared.module';
import { AuthInterceptor } from './shared/auth.interceptor';
import { NavbarItemsComponent } from './shared/components/navbar/navbar-items/navbar-items.component';
import { HeroComponent } from './shared/components/hero/hero.component';
import { HeaderComponent } from './shared/components/header/header.component';

const INTERCEPTOR_PROVIDER: Provider = {
  provide: HTTP_INTERCEPTORS,
  multi: true,
  useClass: AuthInterceptor
}

@NgModule({
  declarations: [
    AppComponent,
    MainLayoutComponent,
    HeaderComponent,
    NavbarItemsComponent,
    HomePageComponent,
    PostPageComponent,
    BikesPageComponent,
    AboutPageComponent,
    RidePageComponent,
    HeroComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    SharedModule
  ],
  providers: [INTERCEPTOR_PROVIDER],
  bootstrap: [AppComponent]
})
export class AppModule { }
