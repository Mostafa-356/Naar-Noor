import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HeaderComponent } from '../header/header.component';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';

describe('HeaderComponent', () => {
  let component: HeaderComponent;
  let fixture: ComponentFixture<HeaderComponent>;

  beforeEach(async () => {
    localStorage.clear();
    await TestBed.configureTestingModule({
      imports: [HeaderComponent, HttpClientTestingModule, RouterTestingModule],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
    }).compileComponents();

    fixture = TestBed.createComponent(HeaderComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach(() => {
    localStorage.clear();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have mobileMenuOpen false by default', () => {
    expect(component.mobileMenuOpen).toBe(false);
  });

  it('should have userMenuOpen signal false by default', () => {
    expect(component.userMenuOpen()).toBe(false);
  });

  it('should have authModalOpen signal false by default', () => {
    expect(component.authModalOpen()).toBe(false);
  });

  it('toggleMobileMenu() should open mobile menu', () => {
    component.toggleMobileMenu();
    expect(component.mobileMenuOpen).toBe(true);
  });

  it('toggleMobileMenu() twice should close mobile menu', () => {
    component.toggleMobileMenu();
    component.toggleMobileMenu();
    expect(component.mobileMenuOpen).toBe(false);
  });

  it('toggleMobileMenu() should close user menu when opening', () => {
    component.userMenuOpen.set(true);
    component.toggleMobileMenu();
    expect(component.userMenuOpen()).toBe(false);
  });

  it('toggleUserMenu() should open user menu', () => {
    component.toggleUserMenu();
    expect(component.userMenuOpen()).toBe(true);
  });

  it('toggleUserMenu() twice should close user menu', () => {
    component.toggleUserMenu();
    component.toggleUserMenu();
    expect(component.userMenuOpen()).toBe(false);
  });

  it('openAuthModal() should open auth modal', () => {
    component.openAuthModal();
    expect(component.authModalOpen()).toBe(true);
  });

  it('openAuthModal() should close mobile menu and user menu', () => {
    component.mobileMenuOpen = true;
    component.userMenuOpen.set(true);

    component.openAuthModal();

    expect(component.mobileMenuOpen).toBe(false);
    expect(component.userMenuOpen()).toBe(false);
  });

  it('closeAuthModal() should close auth modal', () => {
    component.openAuthModal();
    component.closeAuthModal();
    expect(component.authModalOpen()).toBe(false);
  });

  it('userInitial should return "?" when no user logged in', () => {
    expect(component.userInitial).toBe('?');
  });

  it('onDocumentClick() should close user menu when clicking outside', () => {
    component.userMenuOpen.set(true);

    const mockEvent = {
      target: document.createElement('div'),
    } as unknown as MouseEvent;

    component.onDocumentClick(mockEvent);

    expect(component.userMenuOpen()).toBe(false);
  });
});
