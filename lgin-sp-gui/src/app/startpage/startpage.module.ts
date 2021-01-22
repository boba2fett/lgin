import { NgModule} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { StartpageComponent } from './startpage.component';
import { StartpageRoutingModule } from './startpage-routing.module';
import { CommonModule } from '@angular/common';

@NgModule({
    imports: [
        StartpageRoutingModule,
        FormsModule,
        CommonModule
    ],
    declarations: [
        StartpageComponent
    ]
})
export class StartpageModule { }