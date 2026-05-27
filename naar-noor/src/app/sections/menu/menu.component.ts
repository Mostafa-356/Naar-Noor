import { Component, OnInit, CUSTOM_ELEMENTS_SCHEMA, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService, MenuItem } from '../../services/api.service';
import { RevealDirective } from '../../directives/scroll-reveal.directive';

interface MenuItemView {
  name: string;
  price: string;
  description: string;
  category: string;
  isVegetarian: boolean;
  isVegan: boolean;
}

@Component({
  selector: 'app-menu',
  standalone: true,
  imports: [CommonModule, RevealDirective],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  templateUrl: './menu.component.html',
  styleUrls: ['./menu.component.css']
})
export class MenuComponent implements OnInit {
  private readonly api = inject(ApiService);

  allItems: MenuItemView[] = [];
  filteredItems: MenuItemView[] = [];
  categories: string[] = [];
  activeCategory = 'All';
  loading = true;
  error = false;

  ngOnInit(): void {
    this.api.getMenu().subscribe({
      next: (items: MenuItem[]) => {
        this.allItems = items.map(item => ({
          name: item.name,
          price: `£${item.price.toFixed(2)}`,
          description: item.description,
          category: item.category,
          isVegetarian: item.isVegetarian,
          isVegan: item.isVegan
        }));
        const unique = [...new Set(this.allItems.map(i => i.category))];
        this.categories = ['All', ...unique];
        this.filteredItems = this.allItems;
        this.loading = false;
      },
      error: () => {
        this.error = true;
        this.loading = false;
      }
    });
  }

  setCategory(cat: string): void {
    this.activeCategory = cat;
    this.filteredItems = cat === 'All'
      ? this.allItems
      : this.allItems.filter(i => i.category === cat);
  }
}
