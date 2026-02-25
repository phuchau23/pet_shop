# Docker Commands - FoodBooking API

## Quick Start

### Windows (PowerShell)
```powershell
# Quick rebuild (recommended for code updates)
.\docker-rebuild.ps1

# Full rebuild (no cache - slower but ensures fresh build)
.\docker-build-and-run.ps1
```

### Linux/Mac (Bash)
```bash
# Quick rebuild (recommended for code updates)
chmod +x docker-rebuild.sh
./docker-rebuild.sh

# Full rebuild (no cache - slower but ensures fresh build)
chmod +x docker-build-and-run.sh
./docker-build-and-run.sh
```

## Manual Commands

### Stop containers
```bash
docker-compose down
```

### Rebuild and start (with cache)
```bash
docker-compose up --build -d
```

### Rebuild without cache (fresh build)
```bash
docker-compose build --no-cache api
docker-compose up -d
```

### View logs
```bash
docker-compose logs -f api
```

### Stop and remove everything (including volumes)
```bash
docker-compose down -v
```

## Environment Variables

- `AUTO_MIGRATE=true` - Auto run migrations on startup
- `AUTO_SEED_LOCATIONS=true` - Auto seed location data (with OSM coordinates)
- `ENABLE_SWAGGER=true` - Enable Swagger UI

## Notes

- **First time seed**: Will take ~70 seconds to fetch coordinates from OSM (63 provinces × 1.1s delay)
- **Subsequent runs**: Will skip seed if data already exists
- **API Endpoints**: 
  - API: http://localhost:5000
  - Swagger: http://localhost:5000/swagger
  - Health: http://localhost:5000/health
