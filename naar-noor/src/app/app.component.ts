import { Component, OnInit, inject } from '@angular/core';
import { RouterOutlet, Router, NavigationEnd, NavigationStart } from '@angular/router';
import { filter, take } from 'rxjs/operators';
import { HeaderComponent } from './components/header/header.component';
import { FooterComponent } from './components/footer/footer.component';
import { AnimatedBackgroundComponent } from './components/animated-background/animated-background.component';
import { ToastComponent } from './components/toast/toast.component';
import { CartDrawerComponent } from './components/cart-drawer/cart-drawer.component';
import { CommonModule } from '@angular/common';
import { LanguageService } from './services/language.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, HeaderComponent, FooterComponent, AnimatedBackgroundComponent, ToastComponent, CartDrawerComponent],
  template: `
    <div class="relative min-h-screen" [class.nn-page-ready]="pageReady">
      <app-animated-background [zIndex]="'-z-50'"></app-animated-background>
      <app-header></app-header>
      <main class="nn-page-content">
        <router-outlet></router-outlet>
      </main>
      <app-footer></app-footer>
      <app-toast></app-toast>
      <app-cart-drawer></app-cart-drawer>
    </div>
  `,
  styles: [`
    /* App shell is invisible until first route resolves */
    .nn-page-content {
      opacity: 0;
      transition: opacity 0.3s ease;
    }
    .nn-page-ready .nn-page-content {
      opacity: 1;
    }
  `]
})
export class AppComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly languageService = inject(LanguageService);  // ✅ Initialize i18n
  pageReady = false;

  ngOnInit(): void {
    // LanguageService initialization happens via dependency injection
    // It loads the saved language preference or defaults to 'en'

    this.router.events
      .pipe(
        filter(e => e instanceof NavigationEnd),
        take(1)
      )
      .subscribe(() => {
        // Ensure we're at the top before revealing
        window.scrollTo({ top: 0, behavior: 'instant' as ScrollBehavior });

        // Small tick so Angular finishes rendering before we show
        requestAnimationFrame(() => {
          this.pageReady = true;
          this.removeSplash();
        });
      });
  }

  private removeSplash(): void {
    const splash = document.getElementById('nn-splash');
    if (!splash) return;
    splash.classList.add('nn-fade-out');
    setTimeout(() => splash.remove(), 500);
  }
}
