import { Component, CUSTOM_ELEMENTS_SCHEMA, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { SeoService } from '../../services/seo.service';
import { trigger, transition, style, animate } from '@angular/animations';

@Component({
  selector: 'app-payment-cancelled',
  standalone: true,
  imports: [CommonModule, RouterModule],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  animations: [
    trigger('fadeUp', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(24px)' }),
        animate('500ms 200ms cubic-bezier(0.32,0.72,0,1)', style({ opacity: 1, transform: 'translateY(0)' }))
      ])
    ])
  ],
  template: `
    <div class="min-h-screen bg-[#0a0a0a] flex items-center justify-center px-4 py-20">
      <div class="text-center max-w-lg w-full">

        <!-- Icon -->
        <div class="w-24 h-24 mx-auto mb-8 rounded-full bg-amber-500/10 border border-amber-500/20 flex items-center justify-center">
          <iconify-icon icon="solar:close-circle-bold" width="52" class="text-amber-400"></iconify-icon>
        </div>

        <div @fadeUp>
          <span class="text-amber-500 text-xs font-medium tracking-[0.2em] uppercase mb-3 block">Payment Cancelled</span>
          <h1 class="font-['Forum'] text-3xl sm:text-4xl text-white tracking-tight mb-4">
            No Charge Made
          </h1>
          <p class="text-neutral-400 text-sm leading-relaxed mb-8">
            Your payment was cancelled and you have not been charged. 
            You can go back and try again whenever you're ready.
          </p>

          <!-- Actions -->
          <div class="flex flex-col sm:flex-row gap-3 justify-center">
            <button
              (click)="tryAgain()"
              class="px-6 py-3 bg-[#C65A1E] hover:bg-[#b54e17] text-white text-sm font-medium rounded-xl transition-colors">
              Try Again
            </button>
            <a
              routerLink="/"
              class="px-6 py-3 bg-white/5 hover:bg-white/10 border border-white/10 text-white text-sm font-medium rounded-xl transition-colors text-center">
              Return Home
            </a>
          </div>
        </div>
      </div>
    </div>
  `
})
export class PaymentCancelledComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly seo    = inject(SeoService);

  ngOnInit(): void {
    this.seo.set({ title: 'Payment Cancelled', description: 'Your payment was cancelled. You have not been charged.' });
  }

  tryAgain(): void {
    this.router.navigate(['/checkout']);
  }
}
