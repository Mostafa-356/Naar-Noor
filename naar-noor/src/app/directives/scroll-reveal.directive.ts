import { Directive, ElementRef, Input, OnDestroy, OnInit } from '@angular/core';

@Directive({
  selector: '[reveal]',
  standalone: true
})
export class RevealDirective implements OnInit, OnDestroy {
  @Input() reveal: any;
  @Input() revealDelay = 0;
  @Input() revealFrom: 'bottom' | 'left' | 'right' | 'scale' = 'bottom';

  private observer?: IntersectionObserver;

  constructor(private el: ElementRef<HTMLElement>) {}

  ngOnInit(): void {
    const el = this.el.nativeElement;

    const transforms: Record<string, string> = {
      bottom: 'translateY(36px)',
      left:   'translateX(-36px)',
      right:  'translateX(36px)',
      scale:  'scale(0.92) translateY(16px)'
    };

    el.style.opacity = '0';
    el.style.transform = transforms[this.revealFrom];
    el.style.willChange = 'opacity, transform';

    this.observer = new IntersectionObserver(([entry]) => {
      if (entry.isIntersecting) {
        setTimeout(() => {
          el.style.transition = `opacity 0.65s cubic-bezier(0.22,1,0.36,1) ${this.revealDelay}ms, transform 0.65s cubic-bezier(0.22,1,0.36,1) ${this.revealDelay}ms`;
          el.style.opacity = '1';
          el.style.transform = 'none';
          el.style.willChange = 'auto';
        }, 60);
        this.observer?.disconnect();
      }
    }, { threshold: 0.12 });

    this.observer.observe(el);
  }

  ngOnDestroy(): void {
    this.observer?.disconnect();
  }
}
