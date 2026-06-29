import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CustomCalendarComponent } from '../custom-calendar/custom-calendar.component';
import { DropdownManagerService } from '../../services/dropdown-manager.service';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';

describe('CustomCalendarComponent', () => {
  let component: CustomCalendarComponent;
  let fixture: ComponentFixture<CustomCalendarComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CustomCalendarComponent],
      providers: [DropdownManagerService],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
    }).compileComponents();

    fixture = TestBed.createComponent(CustomCalendarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should be closed by default', () => {
    expect(component.isOpen).toBe(false);
  });

  it('should have no selected date by default', () => {
    expect(component.selectedDate).toBeNull();
  });

  it('generateCalendar() builds weeks for the current month', () => {
    component.generateCalendar();
    expect(component.weeks.length).toBeGreaterThan(0);
    expect(component.weeks[0].length).toBe(7);
  });

  it('generateCalendar() populates correct number of days', () => {
    component.currentMonth = new Date(2024, 0, 1); // January 2024 (31 days)
    component.generateCalendar();

    const allDays = component.weeks.flat().filter(d => d !== null);
    expect(allDays.length).toBe(31);
  });

  it('toggleCalendar() opens the calendar', () => {
    component.toggleCalendar();
    expect(component.isOpen).toBe(true);
  });

  it('toggleCalendar() twice closes the calendar', () => {
    component.toggleCalendar();
    component.toggleCalendar();
    expect(component.isOpen).toBe(false);
  });

  it('previousMonth() navigates to prior month', () => {
    const initial = component.currentMonth.getMonth();
    component.previousMonth();

    const expected = initial === 0 ? 11 : initial - 1;
    expect(component.currentMonth.getMonth()).toBe(expected);
  });

  it('nextMonth() navigates to next month', () => {
    const initial = component.currentMonth.getMonth();
    component.nextMonth();

    const expected = initial === 11 ? 0 : initial + 1;
    expect(component.currentMonth.getMonth()).toBe(expected);
  });

  it('selectDate() sets the selected date for a future date', () => {
    const future = new Date();
    future.setDate(future.getDate() + 7);

    component.selectDate(future);

    expect(component.selectedDate).toEqual(future);
    expect(component.isOpen).toBe(false);
  });

  it('selectDate() ignores null input', () => {
    component.selectDate(null);
    expect(component.selectedDate).toBeNull();
  });

  it('selectDate() ignores past dates', () => {
    const past = new Date(2000, 0, 1);
    component.selectDate(past);
    expect(component.selectedDate).toBeNull();
  });

  it('selectDate() emits dateSelected event for valid future date', () => {
    const spy = jest.fn();
    component.dateSelected.subscribe(spy);

    const future = new Date();
    future.setDate(future.getDate() + 5);
    component.selectDate(future);

    expect(spy).toHaveBeenCalledWith(future);
  });

  it('selectToday() selects today', () => {
    component.selectToday();
    const today = new Date();
    expect(component.selectedDate?.toDateString()).toBe(today.toDateString());
  });

  it('isSelected() returns true for the selected date', () => {
    const future = new Date();
    future.setDate(future.getDate() + 3);
    component.selectedDate = future;

    expect(component.isSelected(future)).toBe(true);
  });

  it('isSelected() returns false when no date selected', () => {
    expect(component.isSelected(new Date())).toBe(false);
  });

  it('isSelected() returns false for null input', () => {
    expect(component.isSelected(null)).toBe(false);
  });

  it('isToday() returns true for today', () => {
    expect(component.isToday(new Date())).toBe(true);
  });

  it('isToday() returns false for past date', () => {
    expect(component.isToday(new Date(2000, 0, 1))).toBe(false);
  });

  it('isToday() returns false for null', () => {
    expect(component.isToday(null)).toBe(false);
  });

  it('isPastDate() returns true for yesterday', () => {
    const yesterday = new Date();
    yesterday.setDate(yesterday.getDate() - 1);
    expect(component.isPastDate(yesterday)).toBe(true);
  });

  it('isPastDate() returns false for tomorrow', () => {
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    expect(component.isPastDate(tomorrow)).toBe(false);
  });

  it('isPastDate() returns false for null', () => {
    expect(component.isPastDate(null)).toBe(false);
  });

  it('getFormattedDate() returns "Select Date" when nothing selected', () => {
    expect(component.getFormattedDate()).toBe('Select Date');
  });

  it('getFormattedDate() returns a formatted string when date is selected', () => {
    component.selectedDate = new Date(2025, 5, 15); // June 15, 2025
    const formatted = component.getFormattedDate();
    expect(formatted).not.toBe('Select Date');
    expect(typeof formatted).toBe('string');
    expect(formatted.length).toBeGreaterThan(0);
  });

  it('closes when a different component triggers closeAll', () => {
    const manager = TestBed.inject(DropdownManagerService);
    component.toggleCalendar();
    expect(component.isOpen).toBe(true);

    manager.closeAllExcept('some-other-id');

    expect(component.isOpen).toBe(false);
  });

  it('should unsubscribe on destroy', () => {
    expect(() => component.ngOnDestroy()).not.toThrow();
  });

  it('monthNames has 12 entries', () => {
    expect(component.monthNames).toHaveLength(12);
  });

  it('dayNames has 7 entries', () => {
    expect(component.dayNames).toHaveLength(7);
  });
});
