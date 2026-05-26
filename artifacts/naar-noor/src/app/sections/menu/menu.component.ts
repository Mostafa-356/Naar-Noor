import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MENU_ITEMS } from '../../../data/menu.data';

@Component({
  selector: 'app-menu',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './menu.component.html',
  styleUrls: ['./menu.component.css']
})
export class MenuComponent {
  menuItems = MENU_ITEMS;
}
