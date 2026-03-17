# 🎮 GameList

Calendario de lanzamientos de videojuegos para el año en curso, con sincronización diaria desde la API de IGDB (Twitch). Filtra por plataforma o busca un juego concreto. Solo juegos AAA — los indie se excluyen del sync.

![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)
![Vue](https://img.shields.io/badge/Vue-3-4FC08D?logo=vuedotjs)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-17-4169E1?logo=postgresql)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker)

## Características

- 📅 Calendario mensual + vista diaria con todos los lanzamientos del año (solo juegos AAA)
- 🔍 Búsqueda por nombre de juego en tiempo real
- 🎯 Filtro por plataforma (PC, PS5, Xbox Series X|S, Switch, Switch 2)
- 🗂️ Filtro por tipo de juego (Juego base, Remake, Remasterización, DLC, Expansión…) — por defecto muestra solo juegos base
- 🏷️ Cada juego muestra si es exclusivo o multiplataforma
- ❤️ Favoritos y marcado de juegos comprados (requiere cuenta)
- 👥 Vista de grupo: ve qué juegos quieren o tienen comprados tus amigos
- 👤 Menú de usuario: avatar, cambio de nombre, lista de favoritos y compras
- 🔒 Autenticación segura — JWT en cookie HttpOnly (sin localStorage)
- 🔄 Sincronización automática diaria con IGDB
- 🌐 Descripciones de juegos traducidas al español automáticamente
- 📱 Diseño responsive — cuadrícula 7 columnas en escritorio, lista en móvil

## Tech Stack

| Capa | Tecnología |
|---|---|
| Backend | .NET 10 / ASP.NET Core |
| Frontend | Vue 3 + Vite (SPA servida desde wwwroot) |
| Base de datos | PostgreSQL 17 (contenedor externo compartido) |
| ORM | EF Core 10 — Code First + Migrations |
| CQRS | MediatR |
| Fuente de datos | IGDB API (Twitch) |
| Traducción | LibreTranslate (contenedor Docker autoalojado) |
| Resiliencia | Polly — retry con backoff exponencial |
| Auth | JWT en cookie HttpOnly (`gl_token`) — SameSite=Lax |
| Reverse Proxy | Caddy (contenedor externo) — TLS automático vía DuckDNS |
| Despliegue | Docker + Docker Compose (2 servicios + externos postgres/caddy) |
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
            ├── calendar/     → DayCell, DayView, DayReleasesModal, MonthNavigator
            ├── game/         → GameDetailModal, ReleaseCard
            ├── filters/      → PlatformFilter, CategoryFilter
            └── user/         → UserMenuDropdown
tests/
└── GameList.Api.Tests/       → 109 tests — integración (WebApplicationFactory + Testcontainers) + unitarios (NSubstitute)
```

## Servicios Docker

| Servicio | Descripción |
|---|---|
| `gamelist-web` | ASP.NET Core + Vue SPA (puerto interno 1080) |
| `gamelist-translate` | LibreTranslate en→es (autoalojado) |

**Servicios externos** (sus propios repos):

| Servicio | Repo | Red Docker |
|---|---|---|
| `postgres` | [postgres](https://github.com/RaYx90/postgres) | `postgres` |
| `caddy-proxy` | [caddy-proxy](https://github.com/RaYx90/caddy-proxy) | `caddy` |

## Requisitos

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (o Docker Engine + Compose)
- Credenciales de la [API de IGDB](https://api-docs.igdb.com/#getting-started) (gratuitas — portal de Twitch)
- PostgreSQL corriendo (contenedor `postgres` en red `postgres`)
- Caddy corriendo (contenedor `caddy-proxy` en red `caddy`) para HTTPS

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
DB_PASSWORD=tu_password_de_gamelist_en_postgres
```

> Las credenciales de IGDB se obtienen en [dev.twitch.tv](https://dev.twitch.tv/console/apps) creando una aplicación con `Client Type = Confidential`.

### 3. Levantar con Docker Compose

```bash
docker compose up -d
```

Levanta **2 servicios**: la aplicación web y el traductor (LibreTranslate). PostgreSQL y Caddy corren como contenedores externos.

La aplicación estará disponible en **https://{tu-dominio-duckdns}:1443** (HTTPS vía Caddy) o directamente en el puerto interno 1080.

En el primer arranque:
1. **LibreTranslate** descarga los modelos de idioma (inglés + español) — puede tardar 2-3 minutos.
2. Una vez listo, el `SyncBackgroundService` sincroniza los juegos desde IGDB.
3. El `TranslationBackgroundService` traduce las descripciones al español en segundo plano (20 juegos cada 15 segundos).

### 4. Registrarse

Para crear una cuenta necesitas el `REGISTRATION_SECRET_CODE` que configuraste en el `.env`.

## Desarrollo local

### Backend (.NET)

```bash
cd src/GameList.Web
dotnet run
```

> Requiere PostgreSQL accesible. Puedes usar el contenedor compartido o uno local.

### Frontend (Vue + Vite)

```bash
cd src/GameList.Web/ClientApp
npm install
npm run dev
```

### Tests

```bash
dotnet test
```

Los tests de integración usan **Testcontainers** — levantan un contenedor de PostgreSQL automáticamente. 109 tests en total (44 integración + 65 unitarios).

## Variables de entorno

| Variable | Descripción | Obligatoria |
|---|---|---|
| `IGDB_CLIENT_ID` | Client ID de la app de Twitch | Si |
| `IGDB_CLIENT_SECRET` | Client Secret de la app de Twitch | Si |
| `REGISTRATION_SECRET_CODE` | Código necesario para registrarse | Si |
| `DB_PASSWORD` | Contraseña del usuario gamelist en PostgreSQL | Si |

## Licencia

MIT
