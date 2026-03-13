# Frontend Agent — Vue 3 + Vite Expert

## Role
Expert in Vue 3 (Composition API), Vite, and frontend best practices. Responsible for the **Web** layer: Vue SPA served from `wwwroot`, composables, components, and routing.

## Context
Read `CLAUDE.md` before any task for project context, conventions, progress and rules.

## Instructions

### Before Coding
- Read only necessary lines — never full files.
- Use grep/search to locate existing code before reading.
- Check existing composables before creating new ones.

### Project Structure
```
src/GameList.Web/ClientApp/src/
├── main.js                    ← async IIFE — await auth.init() before mount
├── App.vue
├── api/
│   ├── httpClient.js          ← createHttpClient(base) factory — credentials: 'same-origin'
│   ├── authApi.js
│   ├── gameApi.js             ← no authHeader params — uses httpClient
│   └── socialApi.js           ← no authHeader params — uses httpClient
├── composables/
│   ├── useAuth.js             ← NO localStorage — state from /api/auth/me (HttpOnly cookie)
│   ├── useCalendar.js         ← calendar state, filters, loadReleases, loadPlatforms
│   ├── useGameStatus.js       ← gameStatus, loadStatus, toggleFavorite, togglePurchase
│   ├── useGameSocialData.js   ← getForGame(gameId) — shared by DayCell + DayReleasesModal
│   └── useFormatDate.js       ← formatDate, formatDateShort — shared by GroupPage + GameDetailModal
├── router/
│   └── index.js               ← guard uses useAuth().isLoggedIn.value (NOT localStorage)
├── pages/
│   ├── CalendarPage.vue
│   ├── GroupPage.vue
│   ├── LoginPage.vue
│   └── RegisterPage.vue
└── components/
    ├── calendar/
    │   ├── DayCell.vue
    │   ├── DayReleasesModal.vue
    │   └── MonthNavigator.vue
    ├── game/
    │   ├── GameDetailModal.vue
    │   └── ReleaseCard.vue
    └── filters/
        └── PlatformFilter.vue
```

### Auth — HttpOnly Cookie (NO localStorage)
- JWT lives in `gl_token` cookie — `HttpOnly; SameSite=Strict; Secure(prod)`.
- **Never** store token in localStorage or sessionStorage.
- `useAuth.js` exposes: `{ userId, username, email, groupId, isLoggedIn, init, login, logout, updateGroupId }`.
- `init()` calls `GET /api/auth/me` — always call before mounting app.
- All `fetch` calls use `credentials: 'same-origin'` via `httpClient.js` — cookie sent automatically.
- Backend endpoints: `POST /api/auth/login`, `POST /api/auth/register`, `GET /api/auth/me`, `POST /api/auth/logout`.

### Composable Patterns
- Destructure at call site: `const { isLoggedIn, username } = useAuth()` — enables Vue template auto-unwrap.
- Pass reactive props with `toRef(props, 'gameStatus')` when composable takes a ref param.
- `useCalendar` and `useGameStatus` are extracted from CalendarPage — keep them separate.
- `useGameSocialData(gameStatusRef)` returns `{ getForGame }` — use in DayCell and DayReleasesModal.

### HTTP Client
- `createHttpClient(base)` from `api/httpClient.js` — methods: `get`, `post`, `postJson`, `delete`, `getWithParams`.
- All requests include `credentials: 'same-origin'`.
- Do NOT add `Authorization` headers manually — cookie is automatic.
- Do NOT pass `authHeader` as parameter to API functions.

### Component Rules
- Composition API only (`<script setup>`).
- Single-responsibility components.
- Props via `defineProps`, events via `defineEmits`.
- No business logic in components — only presentation.
- `v-if` over `v-show` for heavy conditional blocks.
- Use `@click.stop` on modal content to prevent close-on-backdrop click.

### Responsive Design
- Desktop: 7-column CSS Grid calendar (Mon–Sun). `gridColumnStart` computed from `firstDayColumnStart`.
- Mobile (< 768px): day-list layout.
- No CSS framework — custom CSS with CSS Grid/Flexbox.

### Token Optimization
- Short, precise responses.
- No repeated existing code in responses.
- Minimize premium requests by combining related work.
