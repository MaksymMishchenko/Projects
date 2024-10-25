import { NgModule } from "@angular/core";
import { AppComponent } from "./app.component";
import { BrowserModule } from "@angular/platform-browser";
import { FormsModule } from "@angular/forms";
import { MainLayoutComponent } from "./shared/components/main-layout/main-layout.component";
import { AppRoutingModule } from "./app.routing.module";
import { HomePageComponent } from "./home-page/home-page.component";
import { PostPageComponent } from "./post-page/post-page.component";

@NgModule({
    declarations: [
        AppComponent,
        MainLayoutComponent,
        HomePageComponent,
        PostPageComponent
    ],
    imports: [
        BrowserModule,
        FormsModule,
        AppRoutingModule
    ],
    exports: [],
    bootstrap: [AppComponent]
})

export class AppModule {   
}