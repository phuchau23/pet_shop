#!/bin/bash
# Quick rebuild script - rebuilds and restarts containers

echo "Quick rebuild - stopping containers..."
docker-compose down

echo "Rebuilding and starting containers..."
docker-compose up --build -d

echo "Showing logs..."
docker-compose logs -f api
