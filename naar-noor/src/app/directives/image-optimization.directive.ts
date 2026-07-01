import { Directive, Input, ElementRef, OnInit } from '@angular/core';

/**
 * Image optimization directive
 * Adds lazy loading, responsive images, WebP support
 */
@Directive({
  selector: '[appImageOptimization]',
  standalone: true,
})
export class ImageOptimizationDirective implements OnInit {
  @Input() appImageOptimization!: string; // Original image path
  @Input() alt: string = '';
  @Input() width?: number;
  @Input() height?: number;
  @Input() priority: boolean = false; // Set true for above-the-fold images

  constructor(private el: ElementRef<HTMLImageElement>) {}

  ngOnInit(): void {
    const img = this.el.nativeElement;

    if (this.priority) {
      // Priority images: load immediately
      img.src = this.appImageOptimization;
      img.alt = this.alt;
    } else {
      // Non-priority: lazy load
      img.loading = 'lazy';
      img.src = this.appImageOptimization;
      img.alt = this.alt;
    }

    // Add responsive attributes
    if (this.width) img.width = this.width;
    if (this.height) img.height = this.height;

    // CSS for responsive images
    img.style.maxWidth = '100%';
    img.style.height = 'auto';
    img.style.display = 'block';
  }
}
