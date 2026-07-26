# Finance Analysis Platform

Collect, store, analyze, and visualize equity market data. Machine learning lives in a separate app (`MLPipeline_Jordan/`); this platform only reads prediction tables.

## Stack

- **Frontend:** React 19, TypeScript 6, Vite 8, Tailwind 4, React Router, TanStack Query, Axios, React Hook Form, Zod
- **Backend:** ASP.NET Core 10, EF Core, PostgreSQL, Clean Architecture
- **Market data:** Pluggable providers (default: mock; production-ready: Polygon.io)

## Local development

PostgreSQL runs in Docker; API and SPA run natively.

```powershell
# 1. Database
docker compose -f docker-compose.dev.yml up -d

# 2. API (hot reload)
cd backend/src/FinanceAnalysis.Api
dotnet watch run

# 3. SPA (separate terminal)
cd frontend
npm install
npm run dev
```

- SPA: http://localhost:5173  
- API: http://localhost:5088  
- OpenAPI (dev): http://localhost:5088/scalar  

Default connection string points at `localhost:5433` (mapped from the compose file). Migrations and universe sync run on API startup in Development.

### Daily ingest (cron / Task Scheduler)

```powershell
# After the API is running:
./scripts/daily-ingest.ps1
```

Requires `INTERNAL_API_KEY` (or defaults from `appsettings.json` in Development).

## Production (Docker)

```powershell
cp .env.example .env
# Edit POSTGRES_PASSWORD, JWT_SIGNING_KEY, INTERNAL_API_KEY
docker compose up --build
```

- Web UI: http://localhost:8080  
- Internal API (loopback only, for cron): http://127.0.0.1:5080  

`MARKET_DATA_PROVIDER=mock` works with no Polygon key. Set `MARKET_DATA_PROVIDER=polygon` and `POLYGON_API_KEY` for live data.

## Tests

```powershell
cd backend
dotnet test FinanceAnalysis.slnx

cd ../frontend
npm test
```

## Configuration notes

| Setting | Purpose |
|--------|---------|
| `Auth__Jwt__SigningKey` | JWT HMAC key (≥32 bytes) |
| `Security__InternalApiKey` | Cron auth for `/api/internal/*` |
| `MarketData__Provider` | `mock` or `polygon` |
| `MarketData__UniverseFilePath` | Tracked symbols JSON (~300 names) |
| `Ingestion__ApplyMigrationsOnStartup` | Auto-migrate (on in containers) |

Tracked symbols are edited in `backend/config/universe.json` (no code change required).

## Adding a market data provider

1. Implement `IMarketDataProvider` in Infrastructure.
2. Register it with `AddKeyedSingleton` / `AddKeyedScoped` in `AddMarketData`.
3. Add its key to `MarketDataProviderRegistry`.
4. Set `MarketData:Provider` to that key.
