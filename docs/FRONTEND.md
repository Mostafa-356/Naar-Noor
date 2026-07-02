# Frontend Development Guide

## Quick Start

```bash
cd naar-noor
npm install
npm run dev        # Dev server → http://localhost:5000
npm run build      # Production → dist/naar-noor/browser/
npm run test:ci    # Unit tests (single run)
npx cypress open   # E2E tests (interactive)
```

---

## Project Structure

```
naar-noor/src/app/
├── components/          # Shared UI components (header, footer, cart, etc.)
├── pages/               # Page-level components (home, reviews, checkout, etc.)
├── services/            # API, auth, cart, dropdown services
├── models/              # TypeScript interfaces (MenuItem, Chef, Reservation, etc.)
└── environments/        # environment.ts (apiUrl config)
```

See [STRUCTURE.md](STRUCTURE.md) for full file tree.

---

## Adding a Component

```bash
ng generate component components/my-component
```

Each component follows standalone pattern:
```typescript
@Component({
  selector: 'app-my-component',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './my-component.component.html',
  styleUrls: ['./my-component.component.css']
})
export class MyComponent implements OnInit {
  ngOnInit(): void { /* init logic */ }
}
```

---

## Services

All HTTP calls go through `ApiService`. Use `catchError` to gracefully handle backend unavailability:

```typescript
getMenuItems(): Observable<MenuItem[]> {
  return this.http.get<MenuItem[]>(`${this.baseUrl}/api/menu`).pipe(
    catchError(() => of(FALLBACK_MENU))
  );
}
```

---

## Styling

- **Tailwind CSS** utility classes for layout and spacing
- **Custom CSS** in component `.css` files for animations and brand-specific styles
- Dark theme using `bg-[#0a0a0a]` base, `amber`/`yellow` accent colors
- Glassmorphism via `backdrop-blur`, `bg-white/5`, `border-white/10`

---

## Environment Configuration

`src/environments/environment.ts`:
```typescript
export const environment = {
  production: false,
  apiUrl: '',               // Empty = relative URLs via dev server proxy
  supabaseUrl: '...',
  supabaseAnonKey: '...'
};
```

The dev server proxy (`proxy.conf.json`) forwards `/api/**` → `localhost:8080`.

---

## Running Tests

```bash
npm test           # watch mode
npm run test:ci    # CI / single run
npx cypress run    # E2E headless
npx cypress open   # E2E interactive
```

See [TESTING.md](TESTING.md) for coverage targets and E2E spec details.