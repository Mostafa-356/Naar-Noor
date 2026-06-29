import { Component, CUSTOM_ELEMENTS_SCHEMA, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { SeoService } from '../../services/seo.service';
import { trigger, transition, style, animate, keyframes } from '@angular/animations';

@Component({
  selector: 'app-payment-success',
  standalone: true,
  imports: [CommonModule, RouterModule],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  animations: [
    trigger('fadeUp', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(24px)' }),
        animate('500ms 200ms cubic-bezier(0.32,0.72,0,1)', style({ opacity: 1, transform: 'translateY(0)' }))
      ])
    ]),
    trigger('checkmark', [
      transition(':enter', [
        animate('600ms 100ms ease-out', keyframes([
          style({ opacity: 0, transform: 'scale(0)', offset: 0 }),
          style({ opacity: 1, transform: 'scale(1.2)', offset: 0.7 }),
          style({ opacity: 1, transform: 'scale(1)', offset: 1 })
        ]))
      ])
    ])
  ],
  template: `
    <div class="min-h-screen bg-[#0a0a0a] flex items-center justify-center px-4 py-20">
      <div class="text-center max-w-lg w-full">

        <!-- Animated checkmark -->
        <div @checkmark class="w-24 h-24 mx-auto mb-8 rounded-full bg-emerald-500/10 border border-emerald-500/20 flex items-center justify-center">
          <iconify-icon icon="solar:check-circle-bold" width="52" class="text-emerald-400"></iconify-icon>
        </div>

        <div @fadeUp>
          <span class="text-[#C65A1E] text-xs font-medium tracking-[0.2em] uppercase mb-3 block">Payment Confirmed</span>
          <h1 class="font-['Forum'] text-3xl sm:text-4xl text-white tracking-tight mb-4">
            Thank You!
          </h1>
          <p class="text-neutral-400 text-sm leading-relaxed mb-3">
            Your payment was successful and your order has been placed.
          </p>

          <!-- Order ID box -->
          <div *ngIf="shortOrderId" class="inline-flex items-center gap-2 bg-white/5 border border-white/10 rounded-xl px-5 py-3 mb-8">
            <iconify-icon icon="solar:receipt-bold" width="18" class="text-[#C65A1E]"></iconify-icon>
            <span class="text-neutral-400 text-sm">Order</span>
            <span class="text-white text-sm font-medium tracking-wider">{{ shortOrderId }}</span>
          </div>

          <!-- Info card -->
          <div class="bg-[#111] border border-white/5 rounded-2xl p-6 text-left mb-8 space-y-3">
            <div class="flex items-start gap-3">
              <iconify-icon icon="solar:bell-bold" width="18" class="text-[#C65A1E] mt-0.5 shrink-0"></iconify-icon>
              <p class="text-sm text-neutral-400">We'll contact you shortly to confirm your order details.</p>
            </div>
            <div class="flex items-start gap-3">
              <iconify-icon icon="solar:letter-bold" width="18" class="text-[#C65A1E] mt-0.5 shrink-0"></iconify-icon>
              <p class="text-sm text-neutral-400">A payment receipt has been sent to your email by Stripe.</p>
            </div>
          </div>

          <!-- Actions -->
          <div class="flex flex-col sm:flex-row gap-3 justify-center">
            <button
              (click)="goHome()"
              class="px-6 py-3 bg-[#C65A1E] hover:bg-[#b54e17] text-white text-sm font-medium rounded-xl transition-colors">
              Return Home
            </button>
            <a
              routerLink="/menu"
              class="px-6 py-3 bg-white/5 hover:bg-white/10 border border-white/10 text-white text-sm font-medium rounded-xl transition-colors text-center">
              Order More
            </a>
          </div>
        </div>
      </div>
    </div>
  `
})
export class PaymentSuccessComponent implements OnInit {
  private readonly route  = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly seo    = inject(SeoService);

  shortOrderId = '';

  ngOnInit(): void {
    this.seo.set({ title: 'Payment Confirmed', description: 'Your payment was successful and your order has been placed.' });
    const orderId = this.route.snapshot.queryParamMap.get('order_id');
    if (orderId) {
      this.shortOrderId = '#' + orderId.split('-')[0].toUpperCase();
    }
  }

  goHome(): void {
    this.router.navigate(['/']);
  }
}
