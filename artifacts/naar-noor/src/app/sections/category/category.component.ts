import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CATEGORIES_DATA } from '../../../data/category.data';

@Component({
  selector: 'app-category',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './category.component.html',
  styleUrls: ['./category.component.css']
})
export class CategoryComponent {
  categories = CATEGORIES_DATA;
}
