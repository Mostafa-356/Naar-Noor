import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ToastComponent } from '../toast/toast.component';
import { ToastService } from '../../services/toast.service';
import { CUSTOM_ELEMENTS_SCHEMA, signal } from '@angular/core';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

describe('ToastComponent', () => {
  let component: ToastComponent;
  let fixture: ComponentFixture<ToastComponent>;
  let toastService: ToastService;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ToastComponent, NoopAnimationsModule],
      providers: [ToastService],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
    }).compileComponents();

    toastService = TestBed.inject(ToastService);
    fixture = TestBed.createComponent(ToastComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('successToasts() returns only success type toasts', () => {
    toastService.success('Success!');
    toastService.error('Error!');

    const successes = component.successToasts();
    expect(successes.length).toBe(1);
    expect(successes[0].message).toBe('Success!');
    expect(successes[0].type).toBe('success');
  });

  it('otherToasts() returns error and warning toasts', () => {
    toastService.success('Success!');
    toastService.error('Error!');
    toastService.warning('Warning!');

    const others = component.otherToasts();
    expect(others.length).toBe(2);
    expect(others.every(t => t.type !== 'success')).toBe(true);
  });

  it('otherToasts() returns empty when only success toasts exist', () => {
    toastService.success('Just success');
    expect(component.otherToasts().length).toBe(0);
  });

  it('successToasts() returns empty when no success toasts', () => {
    toastService.error('Just an error');
    expect(component.successToasts().length).toBe(0);
  });

  it('trackById() returns the toast id', () => {
    const toast = { id: 42, message: 'Hello', type: 'info' as const, duration: 4000 };
    expect(component.trackById(0, toast)).toBe(42);
  });

  it('iconFor() returns correct icon for error', () => {
    expect(component.iconFor('error')).toBe('solar:close-circle-bold');
  });

  it('iconFor() returns correct icon for warning', () => {
    expect(component.iconFor('warning')).toBe('solar:danger-triangle-bold');
  });

  it('iconFor() returns correct icon for info', () => {
    expect(component.iconFor('info')).toBe('solar:info-circle-bold');
  });

  it('iconFor() returns default icon for unknown type', () => {
    expect(component.iconFor('unknown-type')).toBe('solar:info-circle-bold');
  });

  it('dismiss() removes toast from service', () => {
    toastService.error('Dismiss me');
    const id = toastService.toasts()[0].id;

    component.dismiss(id);

    const remaining = toastService.toasts().filter(t => t.id === id);
    expect(remaining.length).toBe(0);
  });

  it('should render without errors when multiple toast types exist', () => {
    toastService.success('Ok!');
    toastService.error('Fail!');
    toastService.warning('Careful!');
    toastService.info('Note');

    fixture.detectChanges();
    expect(component).toBeTruthy();
  });
});
