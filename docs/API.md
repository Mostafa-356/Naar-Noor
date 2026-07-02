# API Reference

**Base URL**
- Dev: `http://localhost:8080/api`
- Prod: `https://naar-noor-api.vercel.app/api`

**Auth:** None required currently (JWT planned for v2)  
**Interactive docs:** `http://localhost:8080/swagger`

---

## Endpoints Summary

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/chefs` | All chefs |
| GET | `/api/menu` | All menu items (filterable) |
| GET | `/api/reservations` | All reservations |
| POST | `/api/reservations` | Create reservation |
| GET | `/api/reviews` | Approved reviews |
| POST | `/api/reviews` | Submit review |
| POST | `/api/contact` | Submit contact inquiry |
| GET | `/health` | API health check |

---

## GET /api/chefs

**Response 200:**
```json
[
  {
    "id": 1,
    "name": "Chef Arjun",
    "specialty": "Indian Cuisine",
    "bio": "Expert in traditional Indian cooking with 15 years of experience",
    "imageUrl": "/assets/chefs/arjun.jpg",
    "createdAt": "2026-05-26T10:00:00Z"
  }
]
```

---

## GET /api/menu

**Query params:** `?category=Mains&available=true&page=1&pageSize=10`

| Param | Type | Description |
|-------|------|-------------|
| `category` | string | `Starters` \| `Mains` \| `Cocktails` |
| `available` | boolean | Filter by availability |
| `page` | int | Page number (default: 1) |
| `pageSize` | int | Items per page (default: 10, max: 100) |

**Response 200:**
```json
[
  {
    "id": 1,
    "name": "Tandoori Chicken",
    "description": "Grilled chicken marinated in yogurt and spices",
    "price": 14.99,
    "category": "Mains",
    "imageUrl": "/assets/menu/tandoori.jpg",
    "isAvailable": true
  }
]
```

---

## GET /api/reservations

**Response 200:**
```json
[
  {
    "id": 1,
    "guestName": "John Doe",
    "email": "john@example.com",
    "phoneNumber": "+1-555-0123",
    "reservationDate": "2026-06-15T19:00:00Z",
    "numberOfGuests": 4,
    "specialRequests": "Window seat preferred",
    "status": "Confirmed"
  }
]
```

## POST /api/reservations

**Request body:**
```json
{
  "guestName": "Jane Smith",
  "email": "jane@example.com",
  "phoneNumber": "+1-555-0456",
  "reservationDate": "2026-06-20T19:30:00Z",
  "numberOfGuests": 2,
  "specialRequests": "Vegetarian options needed"
}
```

**Validation:**

| Field | Rules |
|-------|-------|
| `guestName` | Required, max 100 chars |
| `email` | Required, valid format |
| `phoneNumber` | Required |
| `reservationDate` | Required, must be future |
| `numberOfGuests` | Required, 1–20 |
| `specialRequests` | Optional, max 500 chars |

**Response 201 Created:** Full reservation object with `status: "Pending"`

---

## GET /api/reviews

**Query params:** `?rating=5&limit=10`

**Response 200:**
```json
[
  {
    "id": 1,
    "guestName": "Alice Johnson",
    "rating": 5,
    "comment": "Excellent food and outstanding service!",
    "isApproved": true,
    "createdAt": "2026-05-20T10:00:00Z"
  }
]
```

## POST /api/reviews

```json
{
  "reviewerName": "Test User",
  "rating": 5,
  "comment": "Amazing experience!"
}
```

**Response 201 Created:** Review object (pending approval)

---

## POST /api/contact

**Request body:**
```json
{
  "name": "John Doe",
  "email": "john@example.com",
  "subject": "Catering Inquiry",
  "message": "I would like to inquire about catering for July 15th."
}
```

| Field | Rules |
|-------|-------|
| `name` | Required, max 100 chars |
| `email` | Required, valid format |
| `subject` | Required, max 200 chars |
| `message` | Required, max 1000 chars |

**Response 201 Created:** Contact inquiry object

---

## GET /health

**Response 200:**
```json
{
  "status": "Healthy",
  "timestamp": "2026-05-26T12:00:00Z",
  "version": "1.0.0",
  "database": "Connected"
}
```

---

## HTTP Status Codes

| Code | Meaning |
|------|---------|
| 200 | OK |
| 201 | Created |
| 400 | Bad request / validation error |
| 404 | Not found |
| 500 | Server error |

## Error Response Format

```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Email": ["Email is required"]
  }
}
```

---

## CORS

Allowed origins:
- Dev: `http://localhost:5000`
- Prod: `https://naar-noor.vercel.app`
