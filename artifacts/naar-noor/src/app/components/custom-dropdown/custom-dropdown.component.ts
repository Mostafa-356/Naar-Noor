import { Component, EventEmitter, Output, Input, CUSTOM_ELEMENTS_SCHEMA, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DropdownManagerService } from '../../services/dropdown-manager.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-custom-dropdown',
  standalone: true,
  imports: [CommonModule],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  templateUrl: './custom-dropdown.component.html',
  styleUrls: ['./custom-dropdown.component.css']
})
export class CustomDropdownComponent implements OnInit, OnDestroy {
  @Input() options: string[] = [];
  @Input() selectedValue: string = '';
  @Input() placeholder: string = 'Select option';
  @Input() icon: string = 'solar:alt-arrow-down-linear';
  @Output() valueSelected = new EventEmitter<string>();
  
  isOpen = false;
  private componentId = 'dropdown-' + Math.random().toString(36).substr(2, 9);
  private subscription?: Subscription;

  constructor(private dropdownManager: DropdownManagerService) {}

  ngOnInit() {
    // Subscribe to close all events
    this.subscription = this.dropdownManager.closeAll$.subscribe(exceptId => {
      if (exceptId !== this.componentId) {
        this.isOpen = false;
      }
    });
  }

  ngOnDestroy() {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  toggleDropdown() {
    this.isOpen = !this.isOpen;
    if (this.isOpen) {
      this.dropdownManager.closeAllExcept(this.componentId);
    }
  }

  selectOption(option: string) {
    this.selectedValue = option;
    this.valueSelected.emit(option);
    this.isOpen = false;
  }

  getDisplayValue(): string {
    return this.selectedValue || this.placeholder;
  }
}
