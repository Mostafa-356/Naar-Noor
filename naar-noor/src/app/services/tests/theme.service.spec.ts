import { TestBed } from '@angular/core/testing';
import { ThemeService } from '../theme.service';

describe('ThemeService', () => {
  function createService(storedTheme?: string): ThemeService {
    localStorage.clear();
    if (storedTheme !== undefined) {
      localStorage.setItem('nn_theme', storedTheme);
    }
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({ providers: [ThemeService] });
    return TestBed.inject(ThemeService);
  }

  afterEach(() => {
    localStorage.clear();
  });

  it('should be created', () => {
    const service = createService();
    expect(service).toBeTruthy();
  });

  it('should default to dark theme when no preference stored', () => {
    const service = createService();
    expect(service.isDark()).toBe(true);
  });

  it('should load dark theme from localStorage', () => {
    const service = createService('dark');
    expect(service.isDark()).toBe(true);
  });

  it('should load light theme from localStorage', () => {
    const service = createService('light');
    expect(service.isDark()).toBe(false);
  });

  it('should toggle from dark to light (signal updates immediately)', () => {
    const service = createService('dark');
    service.toggle();
    expect(service.isDark()).toBe(false);
  });

  it('should toggle from light to dark (signal updates immediately)', () => {
    const service = createService('light');
    service.toggle();
    expect(service.isDark()).toBe(true);
  });

  it('should toggle multiple times correctly', () => {
    const service = createService('light');

    service.toggle(); // → dark
    expect(service.isDark()).toBe(true);

    service.toggle(); // → light
    expect(service.isDark()).toBe(false);

    service.toggle(); // → dark
    expect(service.isDark()).toBe(true);
  });

  it('isDark computed signal reflects current theme state', () => {
    const service = createService('light');
    expect(service.isDark()).toBe(false);
    service.toggle();
    expect(service.isDark()).toBe(true);
  });
});
