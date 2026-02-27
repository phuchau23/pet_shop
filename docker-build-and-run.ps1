# PowerShell script to build and run Docker containers
Write-Host "Building and starting FoodBooking containers..." -ForegroundColor Green

# Stop and remove existing containers
Write-Host "Stopping existing containers..." -ForegroundColor Yellow
docker-compose down

# Remove old images to force rebuild (optional - uncomment if needed)
# Write-Host "Removing old API image..." -ForegroundColor Yellow
# docker rmi food-booking-monolith-api 2>$null

# Build and start containers with no cache to ensure fresh build
Write-Host "Building containers with fresh code..." -ForegroundColor Yellow
docker-compose build --no-cache api

Write-Host "Starting containers..." -ForegroundColor Yellow
docker-compose up -d

# Show logs
Write-Host "Showing container logs (press Ctrl+C to exit)..." -ForegroundColor Yellow
docker-compose logs -f api
