# Internationalization (i18n) - Arabic & English Toggle

## Overview

Simple bilingual support for Naar-Noor with **English (EN) and Arabic (AR)** toggle button using **ngx-translate**.

---

## Setup Steps

### Step 1: Install Dependencies

```bash
cd naar-noor
npm install @ngx-translate/core @ngx-translate/http-loader
```

### Step 2: Configure in main.ts

```typescript
import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { AppComponent } from './app/app.component';
import { importProvidersFrom } from '@angular/core';
import { HttpClientModule } from '@angular/common/http';
import { TranslateModule, TranslateLoader } from '@ngx-translate/core';
import { TranslateHttpLoader } from '@ngx-translate/http-loader';
import { HttpClient } from '@angular/common/http';

export function HttpLoaderFactory(http: HttpClient) {
  return new TranslateHttpLoader(http, './assets/i18n/', '.json');
}

bootstrapApplication(AppComponent, {
  providers: [
    ...appConfig.providers,
    importProvidersFrom(HttpClientModule),
    importProvidersFrom(
      TranslateModule.forRoot({
        defaultLanguage: 'en',
        loader: {
          provide: TranslateLoader,
          useFactory: HttpLoaderFactory,
          deps: [HttpClient]
        }
      })
    )
  ]
});
```

### Step 3: Create Translation Files

```bash
mkdir -p src/assets/i18n
```

**en.json**:
```json
{
  "app": {
    "title": "Naar & Noor",
    "tagline": "Authentic Himalayan Restaurant"
  },
  "nav": {
    "home": "Home",
    "menu": "Menu",
    "reservations": "Reservations",
    "contact": "Contact"
  },
  "reservations": {
    "title": "Make a Reservation",
    "date": "Date",
    "time": "Time",
    "partySize": "Party Size",
    "submit": "Reserve Table"
  }
}
```

**ar.json**:
```json
{
  "app": {
    "title": "نار ونور",
    "tagline": "مطعم جبلي أصيل"
  },
  "nav": {
    "home": "الرئيسية",
    "menu": "القائمة",
    "reservations": "الحجوزات",
    "contact": "التواصل"
  },
  "reservations": {
    "title": "احجز طاولة",
    "date": "التاريخ",
    "time": "الوقت",
    "partySize": "عدد الأشخاص",
    "submit": "احجز الطاولة"
  }
}
```

### Step 4: Create Language Service

**src/app/services/language.service.ts**:

```typescript
import { Injectable } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class LanguageService {
  private supportedLanguages = ['en', 'ar'];
  private currentLanguage$ = new BehaviorSubject<string>(this.getInitialLanguage());

  constructor(private translateService: TranslateService) {
    this.initializeTranslation();
  }

  private getInitialLanguage(): string {
    const stored = localStorage.getItem('language');
    if (stored && this.supportedLanguages.includes(stored)) return stored;
    return 'en';
  }

  private initializeTranslation(): void {
    this.translateService.setDefaultLanguage('en');
    this.translateService.addLanguages(this.supportedLanguages);
    this.setLanguage(this.currentLanguage$.value);
  }

  setLanguage(lang: string): void {
    if (!this.supportedLanguages.includes(lang)) return;
    
    this.translateService.use(lang);
    localStorage.setItem('language', lang);
    this.currentLanguage$.next(lang);
    document.documentElement.lang = lang;
    
    // RTL for Arabic
    if (lang === 'ar') {
      document.documentElement.dir = 'rtl';
      document.body.classList.add('rtl');
    } else {
      document.documentElement.dir = 'ltr';
      document.body.classList.remove('rtl');
    }
  }

  getCurrentLanguage(): Observable<string> {
    return this.currentLanguage$.asObservable();
  }
}
```

### Step 5: Create Language Toggle Component

**src/app/components/language-toggle.component.ts**:

```typescript
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LanguageService } from '../services/language.service';

@Component({
  selector: 'app-language-toggle',
  standalone: true,
  imports: [CommonModule],
  template: `
    <button 
      (click)="toggleLanguage()"
      class="toggle-btn"
      [attr.aria-label]="'Switch to ' + getOtherLanguage()"
    >
      {{ getOtherLanguage() }}
    </button>
  `,
  styles: [`
    .toggle-btn {
      padding: 0.5rem 1rem;
      border: 2px solid #333;
      background: white;
      color: #333;
      cursor: pointer;
      border-radius: 4px;
      font-weight: 600;
      transition: all 0.2s;
    }
    .toggle-btn:hover {
      background: #333;
      color: white;
    }
  `]
})
export class LanguageToggleComponent implements OnInit {
  currentLanguage: string = 'en';

  constructor(private languageService: LanguageService) {}

  ngOnInit(): void {
    this.languageService.getCurrentLanguage().subscribe(lang => {
      this.currentLanguage = lang;
    });
  }

  toggleLanguage(): void {
    const newLang = this.currentLanguage === 'en' ? 'ar' : 'en';
    this.languageService.setLanguage(newLang);
  }

  getOtherLanguage(): string {
    return this.currentLanguage === 'en' ? 'العربية' : 'English';
  }
}
```

### Step 6: Use in App

**app.component.ts**:

```typescript
import { Component } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { LanguageToggleComponent } from './components/language-toggle.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [TranslateModule, LanguageToggleComponent],
  template: `
    <header>
      <h1>{{ 'app.title' | translate }}</h1>
      <app-language-toggle></app-language-toggle>
    </header>
    <router-outlet></router-outlet>
  `
})
export class AppComponent {}
```

### Step 7: Add RTL Styles

**global.css**:

```css
/* RTL Support for Arabic */
body.rtl {
  direction: rtl;
  text-align: right;
}

body.rtl nav {
  flex-direction: row-reverse;
}

body.rtl input,
body.rtl textarea {
  text-align: right;
}
```

---

## SEO for Multiple Languages

### Hreflang Tags (index.html)

```html
<link rel="alternate" hrefLang="en" href="https://www.naarnoorresto.com?lang=en">
<link rel="alternate" hrefLang="ar" href="https://www.naarnoorresto.com?lang=ar">
<link rel="alternate" hrefLang="x-default" href="https://www.naarnoorresto.com">
```

### Multilingual Sitemap

```xml
<?xml version="1.0" encoding="UTF-8"?>
<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9"
        xmlns:xhtml="http://www.w3.org/1999/xhtml">
  <url>
    <loc>https://www.naarnoorresto.com</loc>
    <xhtml:link rel="alternate" hrefLang="en" href="https://www.naarnoorresto.com?lang=en"/>
    <xhtml:link rel="alternate" hrefLang="ar" href="https://www.naarnoorresto.com?lang=ar"/>
  </url>
</urlset>
```

---

## Testing

```typescript
describe('LanguageService', () => {
  it('should toggle between EN and AR', (done) => {
    service.setLanguage('ar');
    service.getCurrentLanguage().subscribe(lang => {
      expect(lang).toBe('ar');
      expect(document.documentElement.dir).toBe('rtl');
      done();
    });
  });

  it('should persist language in localStorage', () => {
    service.setLanguage('ar');
    expect(localStorage.getItem('language')).toBe('ar');
  });
});
```

---

## Migration Checklist

- [ ] Install @ngx-translate packages
- [ ] Configure in main.ts
- [ ] Create en.json and ar.json translation files
- [ ] Create LanguageService
- [ ] Create LanguageToggleComponent
- [ ] Add toggle to header/navbar
- [ ] Replace hardcoded text with `{{ 'key' | translate }}`
- [ ] Add RTL styles
- [ ] Update hreflang tags in index.html
- [ ] Test language switching
- [ ] Verify localStorage persistence
- [ ] Test RTL rendering in Arabic mode

