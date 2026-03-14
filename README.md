# 🎮 GameList

Calendario de lanzamientos de videojuegos para el año en curso, con sincronización diaria desde la API de IGDB (Twitch). Filtra por plataforma o busca un juego concreto. Solo juegos AAA — los indie se excluyen del sync.

![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)
![Vue](https://img.shields.io/badge/Vue-3-4FC08D?logo=vuedotjs)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-17-4169E1?logo=postgresql)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker)

## Características

- 📅 Calendario mensual con todos los lanzamientos del año (solo juegos AAA)
- 🔍 Búsqueda por nombre de juego en tiempo real
- 🎯 Filtro por plataforma (PC, PS5, Xbox Series X|S, Switch, Switch 2)
- 🏷️ Cada juego muestra si es exclusivo o multiplataforma
- ❤️ Favoritos y marcado de juegos comprados (requiere cuenta)
- 👥 Vista de grupo: ve qué juegos quieren o tienen comprados tus amigos
- 🔒 Autenticación segura — JWT en cookie HttpOnly (sin localStorage)
- 🔄 Sincronización automática diaria con IGDB
- 🌐 Descripciones de juegos traducidas al español automáticamente
- 📱 Diseño responsive — cuadrícula 7 columnas en escritorio, lista en móvil

## Tech Stack

| Capa | Tecnología |
|---|---|
| Backend | .NET 10 / ASP.NET Core |
| Frontend | Vue 3 + Vite (SPA servida desde wwwroot) |
| Base de datos | PostgreSQL 17 |
| ORM | EF Core 10 — Code First + Migrations |
| CQRS | MediatR |
| Fuente de datos | IGDB API (Twitch) |
| Traducción | LibreTranslate (contenedor Docker autoalojado) |
| Resiliencia | Polly — retry con backoff exponencial |
| Auth | JWT en cookie HttpOnly (`gl_token`) — SameSite=Lax |
| Reverse Proxy | Caddy — TLS automático vía DuckDNS |
| Despliegue | Docker + Docker Compose (4 servicios) |
| Tests | xUnit + WebApplicationFactory + Testcontainers + NSubstitute |

## Arquitectura

Clean Architecture / Hexagonal — 4 capas:

```
src/
├── GameList.Domain/          → Entidades, Value Objects, Ports (interfaces) — sin dependencias
├── GameList.Application/     → Casos de uso (Queries, Commands, DTOs, Mappers)
├── GameList.Infrastructure/  → Adaptadores (EF Core, cliente IGDB, BackgroundService)
└── GameList.Web/             → API mínima, Vue SPA, raíz de DI
    └── ClientApp/src/
        ├── api/              → httpClient factory, gameApi, socialApi, authApi
        ├── composables/      → useAuth, useCalendar, useGameStatus, useGameSocialData, useFormatDate
        ├── pages/            → CalendarPage, GroupPage, LoginPage, RegisterPage
        └── components/
            ├── calendar/     → DayCell, DayReleasesModal, MonthNavigator
            ├── game/         → GameDetailModal, ReleaseCard
            └── filters/      → PlatformFilter
tests/
└── GameList.Api.Tests/       → 69 tests — integración (WebApplicationFactory + Testcontainers) + unitarios (NSubstitute)
```

## Requisitos

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (o Docker Engine + Compose)
- Credenciales de la [API de IGDB](https://api-docs.igdb.com/#getting-started) (gratuitas — se obtienen en el portal de Twitch)

## Puesta en marcha

### 1. Clonar el repositorio

```bash
git clone https://github.com/RaYx90/mygamelist.git
cd mygamelist
```

### 2. Configurar variables de entorno

Crea un fichero `.env` en la raíz del proyecto:

```env
IGDB_CLIENT_ID=tu_client_id
IGDB_CLIENT_SECRET=tu_client_secret
REGISTRATION_SECRET_CODE=codigo_secreto_para_registrarse
DB_PASSWORD=GameList_Prod_2026!
DUCKDNS_DOMAIN=tu-subdominio.duckdns.org
DUCKDNS_TOKEN=tu_token_duckdns
```

> Las credenciales de IGDB se obtienen en [dev.twitch.tv](https://dev.twitch.tv/console/apps) creando una aplicación con `Client Type = Confidential`.

### 3. Levantar con Docker Compose

```bash
docker compose up -d
```

Levanta **4 servicios**: base de datos (PostgreSQL), traductor (LibreTranslate), la aplicación web y el reverse proxy (Caddy).

La aplicación estará disponible en **https://{tu-dominio-duckdns}:1443** (HTTPS vía Caddy) o directamente en el puerto interno 1080 si accedes sin proxy.

En el primer arranque ocurre lo siguiente:
1. **LibreTranslate** descarga los modelos de idioma (inglés + español) — puede tardar 2-3 minutos.
2. Una vez listo, el `SyncBackgroundService` sincroniza los juegos desde IGDB.
3. El `TranslationBackgroundService` traduce las descripciones al español en segundo plano (20 juegos cada 15 segundos) sin bloquear el sync.

> **Nota:** LibreTranslate se autoaloja en el propio Docker Compose — no requiere cuenta ni API key externa.

### 4. Registrarse

Para crear una cuenta necesitas el `REGISTRATION_SECRET_CODE` que configuraste en el `.env`.

## Desarrollo local

### Backend (.NET)

```bash
cd src/GameList.Web
dotnet run
```

> Requiere PostgreSQL en `localhost:5432`. Puedes levantarlo solo con:
> ```bash
> docker compose up -d db
> ```

### Frontend (Vue + Vite)

```bash
cd src/GameList.Web/ClientApp
npm install
npm run dev
```

El servidor de desarrollo de Vite hace proxy al backend en `http://localhost:5000`.

### Tests

```bash
dotnet test
```

Los tests de integración usan **Testcontainers** — levantan un contenedor de PostgreSQL automáticamente, no necesitas nada más. 69 tests en total (39 integración + 30 unitarios).

## Variables de entorno

| Variable | Descripción | Obligatoria |
|---|---|---|
| `IGDB_CLIENT_ID` | Client ID de la app de Twitch | ✅ |
| `IGDB_CLIENT_SECRET` | Client Secret de la app de Twitch | ✅ |
| `REGISTRATION_SECRET_CODE` | Código necesario para registrarse | ✅ |
| `DB_PASSWORD` | Contraseña de PostgreSQL | ⬜ (tiene valor por defecto) |
| `DUCKDNS_DOMAIN` | Subdominio DuckDNS (e.g. `miapp.duckdns.org`) | ✅ (para HTTPS) |
| `DUCKDNS_TOKEN` | Token de DuckDNS para validación TLS | ✅ (para HTTPS) |

## Licencia

MIT
