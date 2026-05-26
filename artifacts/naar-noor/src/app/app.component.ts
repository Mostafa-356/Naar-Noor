import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { HeaderComponent } from './components/header/header.component';
import { FooterComponent } from './components/footer/footer.component';
import { AnimatedBackgroundComponent } from './components/animated-background/animated-background.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, HeaderComponent, FooterComponent, AnimatedBackgroundComponent],
  template: `
    <div class="relative min-h-screen">
      <!-- Global Animated Background -->
      <app-animated-background [zIndex]="'-z-50'"></app-animated-background>
      
      <app-header></app-header>
      <router-outlet></router-outlet>
      <app-footer></app-footer>
    </div>
  `,
  styles: []
})
export class AppComponent {
  title = 'Naar & Noor';

  ngOnInit() {
    // Force scroll to top on load to prevent any browser scroll restoration issues
    window.scrollTo(0, 0);
  }
}
