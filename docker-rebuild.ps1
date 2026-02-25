# Quick rebuild script - rebuilds and restarts containers
Write-Host "Quick rebuild - stopping containers..." -ForegroundColor Yellow
docker-compose down

Write-Host "Rebuilding and starting containers..." -ForegroundColor Green
docker-compose up --build -d

Write-Host "Showing logs..." -ForegroundColor Cyan
docker-compose logs -f api
