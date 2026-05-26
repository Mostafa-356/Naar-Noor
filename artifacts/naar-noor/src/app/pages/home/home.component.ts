import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HeroComponent } from '../../sections/hero/hero.component';
import { ReservationComponent } from '../../sections/reservation/reservation.component';
import { AboutComponent } from '../../sections/about/about.component';
import { CategoryComponent } from '../../sections/category/category.component';
import { MenuComponent } from '../../sections/menu/menu.component';
import { CinematicBannerComponent } from '../../sections/cinematic-banner/cinematic-banner.component';
import { ChefsComponent } from '../../sections/chefs/chefs.component';
import { ReviewsComponent } from '../../sections/reviews/reviews.component';
import { BlogComponent } from '../../sections/blog/blog.component';
import { LocationsComponent } from '../../sections/locations/locations.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    HeroComponent,
    ReservationComponent,
    AboutComponent,
    CategoryComponent,
    MenuComponent,
    CinematicBannerComponent,
    ChefsComponent,
    ReviewsComponent,
    BlogComponent,
    LocationsComponent
  ],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent {}
