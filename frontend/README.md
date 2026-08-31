# Frontend

React + TypeScript + Vite customer app for ArunayanDairy.

The UI is not in Docker Compose yet. Run it on the host against the published backend ports.

## Project layout

```
frontend/
├── README.md
├── package.json
├── vite.config.ts
├── index.html
└── src/
    ├── main.tsx
    ├── App.tsx
    ├── pages/
    │   ├── Login.tsx
    │   ├── Products.tsx
    │   ├── Cart.tsx
    │   └── Orders.tsx
    ├── components/
    │   ├── Header.tsx
    │   └── ErrorBoundary.tsx
    ├── context/CartContext.tsx
    ├── services/
    │   ├── api.ts
    │   ├── userService.ts
    │   ├── productService.ts
    │   └── orderService.ts
    └── lib/userStorage.ts
```

## API bases

Configured in `src/services/api.ts` for Docker Compose host ports:

| Service | URL |
|---|---|
| User | http://localhost:5080 |
| Product | http://localhost:5001 |
| Order | http://localhost:5002 |

If you run APIs with `dotnet` instead of Compose, change these to 5051 / 5296 / 5275.

## Run

Start the backends first (`docker compose up --build` from the repo root).

```bash
cd frontend
npm install
npm run build
npm run start
```

Open http://localhost:5173.

`npm run start` serves a production build (`vite preview` on port 5173). Prefer that for demos. `npm run dev` uses Vite HMR and has been unreliable in this app.

## What is wired

- Register / login (JWT stored locally)
- Product list with quantity controls
- Cart (header count is number of product lines, not total units)
- Place order and view orders
- Sign out
