import { Component, OnInit, inject, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { SeoService } from '../../services/seo.service';
import { AboutComponent } from '../../sections/about/about.component';
import { ChefsComponent } from '../../sections/chefs/chefs.component';
import { RevealDirective } from '../../directives/scroll-reveal.directive';

@Component({
  selector: 'app-about-page',
  standalone: true,
  imports: [CommonModule, RouterModule, AboutComponent, ChefsComponent, RevealDirective],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  template: `
    <!-- Page hero -->
    <div class="pt-32 pb-10 px-6 bg-[#0a0a0a]">
      <div class="max-w-3xl mx-auto text-center space-y-4">
        <span class="text-[#C65A1E] text-xs font-medium tracking-[0.2em] uppercase">Our Story</span>
        <h1 class="font-['Forum'] text-4xl sm:text-5xl text-white tracking-tight">About Naar &amp; Noor</h1>
        <p class="text-neutral-400 text-sm sm:text-base leading-relaxed font-light max-w-xl mx-auto">
          Naar &amp; Noor represents the collision of fire and light — a premium dining experience rooted in
          centuries-old Himalayan culinary traditions, reimagined for the modern palate.
        </p>
      </div>
    </div>

    <!-- Full about section (story + features) -->
    <app-about [standalone]="true"></app-about>

    <!-- Values strip -->
    <section class="py-16 px-6 bg-[#0d0d0d]">
      <div class="max-w-5xl mx-auto">
        <div reveal [revealDelay]="0" class="text-center mb-12">
          <span class="text-[#C65A1E] text-xs font-medium tracking-[0.2em] uppercase mb-3 block">What Drives Us</span>
          <h2 class="font-['Forum'] text-3xl sm:text-4xl text-white tracking-tight">Our Philosophy</h2>
        </div>
        <div reveal [revealDelay]="80" revealFrom="bottom" class="grid grid-cols-1 sm:grid-cols-3 gap-6">
          <div *ngFor="let v of values" class="p-6 rounded-2xl bg-[#111] border border-white/5 space-y-3">
            <div class="w-10 h-10 rounded-lg bg-[#C65A1E]/10 border border-[#C65A1E]/20 flex items-center justify-center text-[#C65A1E]">
              <iconify-icon [attr.icon]="v.icon" width="22"></iconify-icon>
            </div>
            <h3 class="font-['Forum'] text-lg text-white">{{ v.title }}</h3>
            <p class="text-xs text-neutral-400 leading-relaxed font-light">{{ v.description }}</p>
          </div>
        </div>
      </div>
    </section>

    <!-- Chefs -->
    <app-chefs></app-chefs>

    <!-- CTA -->
    <section class="py-16 px-6 bg-[#0d0d0d] text-center">
      <div class="max-w-xl mx-auto space-y-5">
        <h2 class="font-['Forum'] text-3xl text-white">Ready to Experience It?</h2>
        <p class="text-sm text-neutral-400 font-light leading-relaxed">
          Book a table and let our chefs take you on a journey through the flavors of the Himalayas.
        </p>
        <div class="flex flex-col sm:flex-row gap-3 justify-center">
          <a routerLink="/reservations"
             class="px-8 py-3.5 text-sm font-medium text-white bg-[#C65A1E] rounded-xl hover:bg-[#a84915] hover:shadow-[0_0_28px_rgba(198,90,30,0.4)] transition-all duration-300">
            Reserve a Table
          </a>
          <a routerLink="/menu"
             class="px-8 py-3.5 text-sm font-medium text-white border border-white/20 rounded-xl hover:bg-white/5 transition-all duration-300">
            Explore the Menu
          </a>
        </div>
      </div>
    </section>
  `
})
export class AboutPageComponent implements OnInit {
  private readonly seo = inject(SeoService);

  values = [
    {
      icon: 'solar:fire-bold',
      title: 'Ancient Techniques',
      description: 'Every dish is prepared using methods handed down through generations of Himalayan cooks — slow cooking, open flame, and time-honored spice blending.'
    },
    {
      icon: 'solar:leaf-bold',
      title: 'Seasonal Freshness',
      description: 'We source locally where possible and rotate our menu with the seasons, ensuring every ingredient is at its peak when it reaches your plate.'
    },
    {
      icon: 'solar:heart-bold',
      title: 'Hospitality First',
      description: 'From the moment you walk in to the final bite, your experience is our focus. We believe exceptional food deserves equally exceptional care.'
    }
  ];

  ngOnInit(): void {
    this.seo.set({ title: 'About' });
  }
}
