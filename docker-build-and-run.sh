#!/bin/bash
# Bash script to build and run Docker containers

echo "Building and starting FoodBooking containers..."

# Stop and remove existing containers
echo "Stopping existing containers..."
docker-compose down

# Remove old images to force rebuild (optional - uncomment if needed)
# echo "Removing old API image..."
# docker rmi food-booking-monolith-api 2>/dev/null

# Build and start containers with no cache to ensure fresh build
echo "Building containers with fresh code..."
docker-compose build --no-cache api

echo "Starting containers..."
docker-compose up -d

# Show logs
echo "Showing container logs (press Ctrl+C to exit)..."
docker-compose logs -f api
