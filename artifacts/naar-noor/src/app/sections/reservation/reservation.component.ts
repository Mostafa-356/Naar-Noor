import { Component, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CustomCalendarComponent } from '../../components/custom-calendar/custom-calendar.component';
import { CustomDropdownComponent } from '../../components/custom-dropdown/custom-dropdown.component';

interface ReservationForm {
  fullName: string;
  phone: string;
  date: Date | null;
  time: string;
  guests: string;
}

@Component({
  selector: 'app-reservation',
  standalone: true,
  imports: [CommonModule, FormsModule, CustomCalendarComponent, CustomDropdownComponent],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  templateUrl: './reservation.component.html',
  styleUrls: ['./reservation.component.css']
})
export class ReservationComponent {
  reservation: ReservationForm = {
    fullName: '',
    phone: '',
    date: null,
    time: '18:00',
    guests: '2 People'
  };

  timeSlots = ['18:00', '18:30', '19:00', '19:30', '20:00', '20:30', '21:00', '21:30'];
  guestOptions = ['1 Person', '2 People', '3 People', '4 People', '5 People', '6 People', '7 People', '8 People'];

  onDateSelected(date: Date) {
    this.reservation.date = date;
  }

  onTimeSelected(time: string) {
    this.reservation.time = time;
  }

  onGuestsSelected(guests: string) {
    this.reservation.guests = guests;
  }

  onSubmit() {
    if (!this.reservation.fullName || !this.reservation.phone || !this.reservation.date) {
      alert('Please fill in all required fields.');
      return;
    }
    
    console.log('Reservation submitted:', this.reservation);
    alert(`Thank you ${this.reservation.fullName}! Your reservation for ${this.reservation.guests} on ${this.reservation.date.toDateString()} at ${this.reservation.time} has been received. We'll contact you at ${this.reservation.phone} to confirm.`);
  }
}
