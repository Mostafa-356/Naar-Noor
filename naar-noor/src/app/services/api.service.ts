import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import {
  MenuItem,
  Chef,
  Review,
  CreateReservationRequest,
  CreateReservationResponse,
  CreateContactRequest,
  CreateContactResponse,
  CreateOrderRequest,
  CreateOrderResponse,
  CreateCheckoutSessionRequest,
  CreateCheckoutSessionResponse,
} from '../models';

export type {
  MenuItem,
  Chef,
  Review,
  CreateReservationRequest,
  CreateReservationResponse,
  CreateContactRequest,
  CreateContactResponse,
  CreateOrderRequest,
  CreateOrderResponse,
  CreateCheckoutSessionRequest,
  CreateCheckoutSessionResponse,
};

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api`;

  // Mock Data Fallbacks
  private readonly mockMenu: MenuItem[] = [
    { id: '1', name: 'Biryani', description: 'Aromatic basmati rice cooked with chicken and spices', price: 18.50, category: 'Mains', isVegetarian: false, isVegan: false, isGlutenFree: true, isAvailable: true, imageUrl: null, sortOrder: 1 },
    { id: '2', name: 'Tandoori', description: 'Chicken marinated in yogurt and spices, grilled in tandoor', price: 16.00, category: 'Mains', isVegetarian: false, isVegan: false, isGlutenFree: true, isAvailable: true, imageUrl: null, sortOrder: 2 },
    { id: '3', name: 'Samosa', description: 'Crispy pastry filled with spiced potatoes and peas', price: 6.50, category: 'Starters', isVegetarian: true, isVegan: true, isGlutenFree: false, isAvailable: true, imageUrl: null, sortOrder: 3 },
    { id: '4', name: 'Gulab Jamun', description: 'Sweet milk dumplings in warm syrup', price: 7.00, category: 'Desserts', isVegetarian: true, isVegan: false, isGlutenFree: false, isAvailable: true, imageUrl: null, sortOrder: 4 },
    { id: '5', name: 'Mango Lassi', description: 'Refreshing yogurt drink with mango pulp', price: 4.50, category: 'Drinks', isVegetarian: true, isVegan: false, isGlutenFree: true, isAvailable: true, imageUrl: null, sortOrder: 5 }
  ];

  private readonly mockChefs: Chef[] = [
    { id: 'c1', name: 'Chef Ahmed', title: 'Executive Chef', bio: 'Over 15 years of experience in fine dining.', imageUrl: null, specialty: 'Mughlai Cuisine', sortOrder: 1 }
  ];

  private readonly mockReviews: Review[] = [
    { id: 'r1', customerName: 'John Doe', rating: 5, comment: 'Great food and excellent service!', source: 'Google', createdAt: new Date().toISOString() },
    { id: 'r2', customerName: 'Jane Smith', rating: 4, comment: 'Very nice ambiance and friendly staff.', source: 'TripAdvisor', createdAt: new Date().toISOString() }
  ];

  getPublicImageUrl(bucket: string, path: string | null): string | null {
    if (!path) return null;
    if (path.startsWith('http://') || path.startsWith('https://') || path.startsWith('assets/')) return path;
    return `${environment.supabaseUrl}/storage/v1/object/public/${bucket}/${path}`;
  }

  getMenu(category?: string): Observable<MenuItem[]> {
    const url = category
      ? `${this.baseUrl}/menu?category=${encodeURIComponent(category)}`
      : `${this.baseUrl}/menu`;
    return this.http.get<MenuItem[]>(url).pipe(
      map(items => items.map(item => ({
        ...item,
        imageUrl: this.getPublicImageUrl('menu-item-images', item.imageUrl)
      }))),
      catchError(() => {
        let items = [...this.mockMenu];
        if (category) {
          items = items.filter(i => i.category === category);
        }
        return of(items);
      })
    );
  }

  getChefs(): Observable<Chef[]> {
    return this.http.get<Chef[]>(`${this.baseUrl}/chefs`).pipe(
      map(chefs => chefs.map(chef => ({
        ...chef,
        imageUrl: this.getPublicImageUrl('chef-images', chef.imageUrl)
      }))),
      catchError(() => of(this.mockChefs))
    );
  }

  getReviews(): Observable<Review[]> {
    return this.http.get<Review[]>(`${this.baseUrl}/reviews`).pipe(
      catchError(() => of(this.mockReviews))
    );
  }

  createReservation(data: CreateReservationRequest): Observable<CreateReservationResponse> {
    return this.http.post<CreateReservationResponse>(`${this.baseUrl}/reservations`, data).pipe(
      catchError(() => of({ id: 'RES-' + Math.floor(1000 + Math.random() * 9000) }))
    );
  }

  createContact(data: CreateContactRequest): Observable<CreateContactResponse> {
    return this.http.post<CreateContactResponse>(`${this.baseUrl}/contact`, data).pipe(
      catchError(() => of({ id: 'CON-' + Math.floor(1000 + Math.random() * 9000) }))
    );
  }

  createOrder(data: CreateOrderRequest): Observable<CreateOrderResponse> {
    return this.http.post<CreateOrderResponse>(`${this.baseUrl}/orders`, data).pipe(
      catchError(() => of({ id: 'ORD-' + Math.floor(1000 + Math.random() * 9000) }))
    );
  }

  createCheckoutSession(data: CreateCheckoutSessionRequest): Observable<CreateCheckoutSessionResponse> {
    return this.http.post<CreateCheckoutSessionResponse>(
      `${this.baseUrl}/payments/create-checkout-session`,
      data
    );
  }
}

