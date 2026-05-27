import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { HeaderComponent } from './components/header/header.component';
import { FooterComponent } from './components/footer/footer.component';
import { AnimatedBackgroundComponent } from './components/animated-background/animated-background.component';
import { ToastComponent } from './components/toast/toast.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, HeaderComponent, FooterComponent, AnimatedBackgroundComponent, ToastComponent],
  template: `
    <div class="relative min-h-screen">
      <app-animated-background [zIndex]="'-z-50'"></app-animated-background>
      <app-header></app-header>
      <router-outlet></router-outlet>
      <app-footer></app-footer>
      <app-toast></app-toast>
    </div>
  `,
  styles: []
})
export class AppComponent {
  title = 'Naar & Noor';
}
