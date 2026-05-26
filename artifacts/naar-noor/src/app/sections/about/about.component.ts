import { Component, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-about',
  standalone: true,
  imports: [CommonModule],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  templateUrl: './about.component.html',
  styleUrls: ['./about.component.css']
})
export class AboutComponent {
  features = [
    {
      icon: 'solar:flame-linear',
      title: 'Flame-Grilled',
      description: 'Specialties cooked over open flames for deep flavor.'
    },
    {
      icon: 'solar:leaf-linear',
      title: 'Fresh Ingredients',
      description: 'Locally sourced produce mixed with Himalayan herbs.'
    },
    {
      icon: 'solar:star-linear',
      title: 'Authentic Flavors',
      description: 'Recipes passed down through generations.'
    },
    {
      icon: 'solar:cup-hot-linear',
      title: 'Premium Ambience',
      description: 'A dark, warm setting perfect for any occasion.'
    }
  ];
}
