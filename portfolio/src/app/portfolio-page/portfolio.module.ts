import { NgModule } from "@angular/core";
import { PortfolioPageComponent } from "./portfolio-page.component";
import { CommonModule } from "@angular/common";
import { BrowserModule } from "@angular/platform-browser";
import { RouterModule } from "@angular/router";

@NgModule({
    declarations: [PortfolioPageComponent],
    imports: [
        CommonModule,
        RouterModule.forChild([
            { path: '', component: PortfolioPageComponent }
        ])
    ],
    exports: [RouterModule]
})

export class PortfolioPageModule {

}