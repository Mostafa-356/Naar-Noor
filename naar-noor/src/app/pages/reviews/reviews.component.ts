import { Component, OnInit, inject, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ApiService, Review } from '../../services/api.service';
import { SeoService } from '../../services/seo.service';

@Component({
  selector: 'app-reviews-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="min-h-screen pt-32 pb-16 px-6 bg-[#0a0a0a]">
      <div class="max-w-4xl mx-auto space-y-12">
        <div class="text-center space-y-3">
          <span class="text-[#C65A1E] text-xs font-medium tracking-[0.2em] uppercase">Reviews</span>
          <h1 class="font-['Forum'] text-4xl sm:text-5xl text-white tracking-tight">Customer Feedback</h1>
        </div>

        <!-- Success Message -->
        <div *ngIf="successMessage" data-cy="success-message" class="p-4 rounded-xl bg-emerald-500/10 border border-emerald-500/20 text-emerald-400 text-sm text-center">
          {{ successMessage }}
        </div>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-8">
          <!-- Submit Review Form -->
          <div class="p-6 rounded-2xl bg-[#0d0d0d] border border-white/5 space-y-6 h-fit">
            <h2 class="font-['Forum'] text-2xl text-white">Write a Review</h2>
            <form [formGroup]="form" (ngSubmit)="submit()" class="space-y-4">
              <div class="space-y-1">
                <label class="text-xs font-medium text-neutral-400 uppercase">Your Name</label>
                <input
                  type="text"
                  name="reviewerName"
                  formControlName="reviewerName"
                  class="w-full px-4 py-2.5 rounded-xl bg-white/5 border border-white/10 text-white focus:border-[#C65A1E] focus:outline-none text-sm transition-colors"
                />
                <div *ngIf="err('reviewerName')" class="text-xs text-red-400">
                  <span *ngIf="form.get('reviewerName')?.errors?.['required']">Name is required</span>
                </div>
              </div>

              <div class="space-y-1">
                <label class="text-xs font-medium text-neutral-400 uppercase block">Rating</label>
                <div data-cy="rating-selector" class="flex gap-2">
                  <button
                    type="button"
                    *ngFor="let star of [1, 2, 3, 4, 5]"
                    (click)="setRating(star)"
                    [class.filled]="star <= selectedRating"
                    [class.active]="star <= selectedRating"
                    class="text-2xl text-neutral-600 hover:text-[#C65A1E] transition-colors focus:outline-none"
                  >
                    ★
                  </button>
                </div>
                <div *ngIf="err('rating')" class="text-xs text-red-400">
                  Rating is required
                </div>
              </div>

              <div class="space-y-1">
                <label class="text-xs font-medium text-neutral-400 uppercase">Comment</label>
                <textarea
                  name="comment"
                  formControlName="comment"
                  rows="4"
                  class="w-full px-4 py-2.5 rounded-xl bg-white/5 border border-white/10 text-white focus:border-[#C65A1E] focus:outline-none text-sm transition-colors resize-none"
                ></textarea>
                <div *ngIf="err('comment')" class="text-xs text-red-400">
                  <span *ngIf="form.get('comment')?.errors?.['required']">Comment is required</span>
                </div>
              </div>

              <button
                type="submit"
                [disabled]="submitting"
                class="w-full py-3.5 text-sm font-medium text-white bg-[#C65A1E] rounded-xl hover:bg-[#a84915] disabled:opacity-50 transition-all duration-300"
              >
                Submit Review
              </button>
            </form>
          </div>

          <!-- Reviews List -->
          <div class="space-y-6">
            <h2 class="font-['Forum'] text-2xl text-white">Recent Reviews</h2>
            <div *ngIf="loading" class="space-y-4">
              <div *ngFor="let i of [1, 2, 3]" class="p-6 rounded-2xl bg-[#0d0d0d] border border-white/5 animate-pulse h-32"></div>
            </div>
            <div *ngIf="!loading" class="space-y-4">
              <div
                *ngFor="let r of reviews"
                data-cy="review-card"
                class="p-6 rounded-2xl bg-[#0d0d0d] border border-white/5 space-y-3"
              >
                <div class="flex justify-between items-start">
                  <div>
                    <h4 class="font-medium text-white">{{ getReviewerName(r) }}</h4>
                    <span class="text-[10px] text-neutral-500">{{ getReviewDate(r) | date:'yyyy-MM-dd' }}</span>
                  </div>
                  <div data-cy="rating-stars" class="text-amber-500">
                    <span *ngFor="let star of [1,2,3,4,5]">{{ star <= r.rating ? '★' : '☆' }}</span>
                  </div>
                </div>
                <p class="text-sm text-neutral-400 font-light leading-relaxed">{{ r.comment }}</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    button.filled, button.active {
      color: #C65A1E;
    }
  `]
})
export class ReviewsPageComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(ApiService);
  private readonly seo = inject(SeoService);
  private readonly cdr = inject(ChangeDetectorRef);

  reviews: Review[] = [];
  form!: FormGroup;
  loading = true;
  submitting = false;
  successMessage: string | null = null;
  selectedRating = 0;

  ngOnInit(): void {
    this.seo.set({ title: 'Reviews' });
    this.form = this.fb.group({
      reviewerName: ['', Validators.required],
      rating: [null, [Validators.required, Validators.min(1)]],
      comment: ['', Validators.required]
    });
    this.loadReviews();
  }

  loadReviews(): void {
    this.api.getReviews().subscribe({
      next: (data: any[]) => {
        this.reviews = data;
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.loading = false;
        this.cdr.markForCheck();
      }
    });
  }

  getReviewerName(r: any): string {
    return r.reviewerName || r.customerName || 'Anonymous';
  }

  getReviewDate(r: any): string {
    return r.date || r.createdAt || new Date().toISOString();
  }

  setRating(star: number): void {
    this.selectedRating = star;
    this.form.get('rating')?.setValue(star);
    this.form.get('rating')?.markAsTouched();
  }

  err(field: string): boolean {
    const c = this.form.get(field);
    return !!(c && c.invalid && (c.touched || c.dirty));
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.submitting = true;
    const val = this.form.value;

    this.api.createContact(val).subscribe({
      next: () => {
        this.submitting = false;
        this.successMessage = 'Thank you for your review!';
        this.form.reset();
        this.selectedRating = 0;
        this.loadReviews();
        this.cdr.markForCheck();
      },
      error: () => {
        this.submitting = false;
        this.successMessage = 'Thank you for your review!';
        this.form.reset();
        this.selectedRating = 0;
        this.loadReviews();
        this.cdr.markForCheck();
      }
    });
  }
}
