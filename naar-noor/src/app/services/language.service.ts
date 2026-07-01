import { Injectable } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { BehaviorSubject, Observable } from 'rxjs';

/**
 * Language service for managing EN/AR bilingual support
 * - Manages language switching
 * - Persists language preference to localStorage
 * - Handles RTL support for Arabic
 */
@Injectable({ providedIn: 'root' })
export class LanguageService {
  private readonly SUPPORTED_LANGUAGES = ['en', 'ar'];
  private readonly STORAGE_KEY = 'language';
  private readonly DEFAULT_LANGUAGE = 'en';

  private currentLanguage$ = new BehaviorSubject<string>(
    this.getInitialLanguage()
  );

  constructor(private translateService: TranslateService) {
    this.initializeTranslation();
  }

  /**
   * Get initial language from localStorage or use default
   */
  private getInitialLanguage(): string {
    const stored = localStorage.getItem(this.STORAGE_KEY);
    if (stored && this.SUPPORTED_LANGUAGES.includes(stored)) {
      return stored;
    }
    return this.DEFAULT_LANGUAGE;
  }

  /**
   * Initialize translation service
   */
  private initializeTranslation(): void {
    this.translateService.setDefaultLanguage(this.DEFAULT_LANGUAGE);
    this.translateService.addLanguages(this.SUPPORTED_LANGUAGES);
    this.setLanguage(this.currentLanguage$.value);
  }

  /**
   * Set active language
   * - Updates TranslateService
   * - Persists to localStorage
   * - Sets document lang and RTL attributes
   */
  setLanguage(lang: string): void {
    if (!this.SUPPORTED_LANGUAGES.includes(lang)) {
      return;
    }

    this.translateService.use(lang);
    localStorage.setItem(this.STORAGE_KEY, lang);
    this.currentLanguage$.next(lang);
    document.documentElement.lang = lang;

    // RTL support for Arabic
    if (lang === 'ar') {
      document.documentElement.dir = 'rtl';
      document.body.classList.add('rtl');
    } else {
      document.documentElement.dir = 'ltr';
      document.body.classList.remove('rtl');
    }
  }

  /**
   * Get current language as Observable
   */
  getCurrentLanguage(): Observable<string> {
    return this.currentLanguage$.asObservable();
  }

  /**
   * Get current language value synchronously
   */
  getCurrentLanguageValue(): string {
    return this.currentLanguage$.value;
  }

  /**
   * Get supported languages
   */
  getSupportedLanguages(): string[] {
    return [...this.SUPPORTED_LANGUAGES];
  }

  /**
   * Toggle between EN and AR
   */
  toggleLanguage(): void {
    const newLang =
      this.currentLanguage$.value === 'en' ? 'ar' : 'en';
    this.setLanguage(newLang);
  }

  /**
   * Get the "other" language (opposite of current)
   */
  getOtherLanguage(): string {
    return this.currentLanguage$.value === 'en' ? 'ar' : 'en';
  }
}
