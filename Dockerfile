# ── Stage 1: Build Vue frontend ─────────────────────────────────────────────
FROM node:22-alpine AS vue-build
WORKDIR /client
COPY src/GameList.Web/ClientApp/package*.json ./
RUN npm ci
COPY src/GameList.Web/ClientApp/ ./
RUN npm run build
# Vite outDir is '../wwwroot' → output lands at /wwwroot

# ── Stage 2: Build .NET backend ──────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files first (layer cache for restore)
COPY GameList.slnx ./
COPY src/GameList.Domain/GameList.Domain.csproj             src/GameList.Domain/
COPY src/GameList.Application/GameList.Application.csproj   src/GameList.Application/
COPY src/GameList.Infrastructure/GameList.Infrastructure.csproj src/GameList.Infrastructure/
COPY src/GameList.Web/GameList.Web.csproj                   src/GameList.Web/
COPY tests/GameList.Api.Tests/GameList.Api.Tests.csproj     tests/GameList.Api.Tests/

RUN dotnet restore

# Copy remaining source
COPY . .

# Inject Vue build output into wwwroot before publish
COPY --from=vue-build /wwwroot src/GameList.Web/wwwroot/

RUN dotnet publish src/GameList.Web/GameList.Web.csproj \
    -c Release \
    -o /app/publish

# ── Stage 3: Runtime ─────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Non-root user for security
RUN useradd --no-create-home --shell /bin/false appuser \
    && chown -R appuser /app
USER appuser

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "GameList.Web.dll"]
