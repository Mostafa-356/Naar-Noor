import { CartItem } from './cart.model';

/** Individual item line in an order request */
export interface OrderItemRequest {
  menuItemId: string;
  menuItemName: string;
  unitPrice: number;
  quantity: number;
}

/** Full order creation request sent to the API */
export interface CreateOrderRequest {
  customerName: string;
  email: string;
  phoneNumber: string;
  notes?: string;
  type: 'collection' | 'delivery' | 'dine-in';
  deliveryAddress?: string;
  tableReservationName?: string;
  items: OrderItemRequest[];
}

/** Response returned after creating an order */
export interface CreateOrderResponse {
  id: string;
}

/** Request to create a Stripe Checkout Session */
export interface CreateCheckoutSessionRequest {
  customerName: string;
  email: string;
  phoneNumber: string;
  notes?: string;
  type: 'collection' | 'delivery' | 'dine-in';
  deliveryAddress?: string;
  tableReservationName?: string;
  items: OrderItemRequest[];
  successUrl: string;
  cancelUrl: string;
}

/** Response from the backend after creating a Stripe Checkout Session */
export interface CreateCheckoutSessionResponse {
  orderId: string;
  sessionUrl: string;
}

/** Numeric step indicator for the cart drawer flow (1 = cart, 2 = checkout, 3 = confirmation) */
export type DrawerStep = 1 | 2 | 3;
