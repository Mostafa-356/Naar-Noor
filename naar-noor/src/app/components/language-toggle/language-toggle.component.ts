import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LanguageService } from '../../services/language.service';

/**
 * Language toggle component
 * Displays current language and provides button to switch between EN and AR
 * Styled as simple toggle button
 */
@Component({
  selector: 'app-language-toggle',
  standalone: true,
  imports: [CommonModule],
  template: `
    <button
      (click)="toggleLanguage()"
      [attr.aria-label]="'Switch to ' + getOtherLanguageLabel()"
      class="language-toggle-btn"
      type="button"
    >
      {{ getOtherLanguageLabel() }}
    </button>
  `,
  styles: [
    `
      .language-toggle-btn {
        padding: 0.5rem 1rem;
        border: 2px solid currentColor;
        background: transparent;
        color: inherit;
        cursor: pointer;
        border-radius: 4px;
        font-weight: 600;
        font-size: 0.875rem;
        transition: all 0.2s ease;
        white-space: nowrap;
      }

      .language-toggle-btn:hover {
        background: currentColor;
        color: var(--bg-color, white);
        transform: scale(1.05);
      }

      .language-toggle-btn:active {
        transform: scale(0.95);
      }

      @media (max-width: 640px) {
        .language-toggle-btn {
          padding: 0.375rem 0.75rem;
          font-size: 0.75rem;
        }
      }
    `
  ]
})
export class LanguageToggleComponent implements OnInit {
  currentLanguage: string = 'en';

  constructor(private languageService: LanguageService) {}

  ngOnInit(): void {
    // Subscribe to language changes
    this.languageService.getCurrentLanguage().subscribe(lang => {
      this.currentLanguage = lang;
    });
  }

  /**
   * Toggle between EN and AR
   */
  toggleLanguage(): void {
    this.languageService.toggleLanguage();
  }

  /**
   * Get label for the "other" language
   */
  getOtherLanguageLabel(): string {
    return this.currentLanguage === 'en' ? 'العربية' : 'English';
  }
}
