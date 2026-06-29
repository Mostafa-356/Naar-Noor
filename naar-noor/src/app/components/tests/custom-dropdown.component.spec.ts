import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CustomDropdownComponent } from '../custom-dropdown/custom-dropdown.component';
import { DropdownManagerService } from '../../services/dropdown-manager.service';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';

describe('CustomDropdownComponent', () => {
  let component: CustomDropdownComponent;
  let fixture: ComponentFixture<CustomDropdownComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CustomDropdownComponent],
      providers: [DropdownManagerService],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
    }).compileComponents();

    fixture = TestBed.createComponent(CustomDropdownComponent);
    component = fixture.componentInstance;
    component.options = ['Option A', 'Option B', 'Option C'];
    component.placeholder = 'Select an option';
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should be closed by default', () => {
    expect(component.isOpen).toBe(false);
  });

  it('toggleDropdown() should open the dropdown', () => {
    component.toggleDropdown();
    expect(component.isOpen).toBe(true);
  });

  it('toggleDropdown() twice should close the dropdown', () => {
    component.toggleDropdown();
    component.toggleDropdown();
    expect(component.isOpen).toBe(false);
  });

  it('selectOption() should set the selected value', () => {
    component.selectOption('Option B');
    expect(component.selectedValue).toBe('Option B');
  });

  it('selectOption() should close the dropdown', () => {
    component.toggleDropdown();
    expect(component.isOpen).toBe(true);

    component.selectOption('Option A');
    expect(component.isOpen).toBe(false);
  });

  it('selectOption() should emit valueSelected event', () => {
    const spy = jest.fn();
    component.valueSelected.subscribe(spy);

    component.selectOption('Option C');

    expect(spy).toHaveBeenCalledWith('Option C');
  });

  it('getDisplayValue() should return placeholder when nothing selected', () => {
    component.selectedValue = '';
    expect(component.getDisplayValue()).toBe('Select an option');
  });

  it('getDisplayValue() should return selected value when set', () => {
    component.selectedValue = 'Option A';
    expect(component.getDisplayValue()).toBe('Option A');
  });

  it('should unsubscribe on destroy', () => {
    expect(() => component.ngOnDestroy()).not.toThrow();
  });

  it('should close when closeAll event is for a different id', () => {
    const manager = TestBed.inject(DropdownManagerService);
    component.toggleDropdown();
    expect(component.isOpen).toBe(true);

    // Emit closeAll for a different id
    manager.closeAllExcept('some-other-id');
    expect(component.isOpen).toBe(false);
  });
});
