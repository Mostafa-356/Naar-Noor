import { Component, OnInit, inject, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SeoService } from '../../services/seo.service';

@Component({
  selector: 'app-contact-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  template: `
    <div class="min-h-screen pt-32 pb-20 px-6 bg-[#0a0a0a]">
      <div class="max-w-5xl mx-auto">

        <!-- Header -->
        <div class="text-center mb-14 space-y-4">
          <span class="text-[#C65A1E] text-xs font-medium tracking-[0.2em] uppercase">Get In Touch</span>
          <h1 class="font-['Forum'] text-4xl sm:text-5xl text-white tracking-tight">Contact Us</h1>
          <p class="text-sm text-neutral-400 leading-relaxed font-light max-w-md mx-auto">
            Questions, private events, or special arrangements — we'd love to hear from you.
          </p>
        </div>

        <div class="grid grid-cols-1 lg:grid-cols-5 gap-8">

          <!-- Info column -->
          <div class="lg:col-span-2 space-y-6">

            <!-- Address -->
            <div class="p-6 rounded-2xl bg-[#0d0d0d] border border-white/5 space-y-3">
              <div class="flex items-center gap-2 text-[#C65A1E]">
                <iconify-icon icon="solar:map-point-bold" width="18"></iconify-icon>
                <span class="text-[10px] font-medium tracking-widest uppercase text-neutral-400">Address</span>
              </div>
              <p class="text-sm text-neutral-300 font-light leading-relaxed">
                Town Centre,<br/>
                St Peter Port,<br/>
                Guernsey, GY1 2PN
              </p>
            </div>

            <!-- Hours -->
            <div class="p-6 rounded-2xl bg-[#0d0d0d] border border-white/5 space-y-3">
              <div class="flex items-center gap-2 text-[#C65A1E]">
                <iconify-icon icon="solar:clock-circle-bold" width="18"></iconify-icon>
                <span class="text-[10px] font-medium tracking-widest uppercase text-neutral-400">Opening Hours</span>
              </div>
              <div class="grid grid-cols-2 gap-x-4 gap-y-1.5 text-xs text-neutral-300 font-light">
                <span class="text-neutral-500">Lunch</span>   <span>12:00 PM – 2:00 PM</span>
                <span class="text-neutral-500">Dinner</span>  <span>6:00 PM – 9:00 PM</span>
                <span class="text-neutral-500">Monday</span>  <span class="text-[#C65A1E] font-medium">Closed</span>
              </div>
            </div>

            <!-- Phone & Email -->
            <div class="p-6 rounded-2xl bg-[#0d0d0d] border border-white/5 space-y-4">
              <div class="flex items-center gap-3">
                <iconify-icon icon="solar:phone-bold" width="18" class="text-[#C65A1E] shrink-0"></iconify-icon>
                <span class="text-sm text-neutral-300 font-light">+44 (0) 1481 123456</span>
              </div>
              <div class="flex items-center gap-3">
                <iconify-icon icon="solar:letter-bold" width="18" class="text-[#C65A1E] shrink-0"></iconify-icon>
                <span class="text-sm text-neutral-300 font-light">hello&#64;naarnoor.com</span>
              </div>
            </div>

          </div>

          <!-- Contact form -->
          <div class="lg:col-span-3 p-8 rounded-2xl bg-[#0d0d0d] border border-white/5">

            <div *ngIf="sent" class="text-center py-12 space-y-4">
              <div class="w-16 h-16 mx-auto rounded-full bg-emerald-500/10 border border-emerald-500/20 flex items-center justify-center text-emerald-400 text-3xl">✓</div>
              <h3 class="font-['Forum'] text-2xl text-white">Message Sent!</h3>
              <p class="text-sm text-neutral-400 font-light">We'll be in touch within one business day.</p>
              <button (click)="sent = false"
                      class="mt-4 px-6 py-2.5 text-sm text-white border border-white/15 rounded-xl hover:bg-white/5 transition-all">
                Send Another
              </button>
            </div>

            <form *ngIf="!sent" (ngSubmit)="submit()" class="space-y-5" #contactForm="ngForm">
              <h2 class="font-['Forum'] text-2xl text-white mb-6">Send a Message</h2>

              <div class="grid grid-cols-1 sm:grid-cols-2 gap-5">
                <div class="space-y-1.5">
                  <label class="text-xs font-medium text-neutral-400 uppercase">Name</label>
                  <input
                    type="text"
                    name="name"
                    [(ngModel)]="form.name"
                    placeholder="Your name"
                    required
                    class="nn-input"
                  />
                </div>
                <div class="space-y-1.5">
                  <label class="text-xs font-medium text-neutral-400 uppercase">Email</label>
                  <input
                    type="email"
                    name="email"
                    [(ngModel)]="form.email"
                    placeholder="your@email.com"
                    required
                    class="nn-input"
                  />
                </div>
              </div>

              <div class="space-y-1.5">
                <label class="text-xs font-medium text-neutral-400 uppercase">Subject</label>
                <select name="subject" [(ngModel)]="form.subject" class="nn-input">
                  <option value="">Select a topic…</option>
                  <option value="reservation">Reservation enquiry</option>
                  <option value="private">Private event / group booking</option>
                  <option value="feedback">Feedback</option>
                  <option value="other">Other</option>
                </select>
              </div>

              <div class="space-y-1.5">
                <label class="text-xs font-medium text-neutral-400 uppercase">Message</label>
                <textarea
                  name="message"
                  [(ngModel)]="form.message"
                  rows="5"
                  placeholder="Tell us how we can help…"
                  required
                  class="nn-input resize-none"
                ></textarea>
              </div>

              <button
                type="submit"
                [disabled]="submitting || !contactForm.valid"
                class="w-full py-3.5 text-sm font-medium text-white bg-[#C65A1E] rounded-xl hover:bg-[#a84915] hover:shadow-[0_0_24px_rgba(198,90,30,0.4)] transition-all duration-300 disabled:opacity-50 disabled:cursor-not-allowed">
                <span *ngIf="!submitting">Send Message</span>
                <span *ngIf="submitting" class="flex items-center justify-center gap-2">
                  <span class="inline-block w-4 h-4 border-2 border-current border-t-transparent rounded-full animate-spin"></span>
                  Sending…
                </span>
              </button>
            </form>

          </div>
        </div>

      </div>
    </div>
  `
})
export class ContactPageComponent implements OnInit {
  private readonly seo = inject(SeoService);

  form = { name: '', email: '', subject: '', message: '' };
  submitting = false;
  sent = false;

  submit(): void {
    if (!this.form.name || !this.form.email || !this.form.message) return;
    this.submitting = true;
    setTimeout(() => {
      this.submitting = false;
      this.sent = true;
      this.form = { name: '', email: '', subject: '', message: '' };
    }, 800);
  }

  ngOnInit(): void {
    this.seo.set({ title: 'Contact' });
  }
}
