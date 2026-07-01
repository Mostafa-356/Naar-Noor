.PHONY: help setup dev dev-backend dev-frontend test lint build install clean docker-up docker-down

SHELL := /bin/bash

# Colors for output
GREEN := \033[0;32m
YELLOW := \033[1;33m
RED := \033[0;31m
NC := \033[0m # No Color

# Targets
help:
	@echo "🍽️  Naar & Noor - Development Commands"
	@echo ""
	@echo "Setup & Install:"
	@echo "  make setup              Setup development environment"
	@echo "  make install            Install all dependencies"
	@echo ""
	@echo "Development:"
	@echo "  make dev                Start both frontend and backend (requires 2 terminals)"
	@echo "  make dev-backend        Start backend API (port 8080)"
	@echo "  make dev-frontend       Start frontend app (port 5000)"
	@echo ""
	@echo "Testing & Quality:"
	@echo "  make test               Run all tests"
	@echo "  make test-backend       Run backend tests only"
	@echo "  make test-frontend      Run frontend tests only"
	@echo "  make lint               Run linters"
	@echo ""
	@echo "Build & Deployment:"
	@echo "  make build              Build production artifacts"
	@echo "  make build-backend      Build backend"
	@echo "  make build-frontend     Build frontend"
	@echo ""
	@echo "Docker:"
	@echo "  make docker-up          Start services with Docker Compose"
	@echo "  make docker-down        Stop Docker services"
	@echo ""
	@echo "Maintenance:"
	@echo "  make clean              Clean build artifacts"
	@echo "  make audit              Run security audits"
	@echo ""

setup:
	@echo "$(GREEN)🔧 Setting up development environment...$(NC)"
	@bash setup.sh
	@echo "$(GREEN)✅ Setup complete!$(NC)"

install:
	@echo "$(GREEN)📦 Installing dependencies...$(NC)"
	cd api-server && dotnet restore && cd ..
	cd naar-noor && npm ci && cd ..
	@echo "$(GREEN)✅ Dependencies installed$(NC)"

dev:
	@echo "$(YELLOW)⚠️  Development requires two terminals:$(NC)"
	@echo "  Terminal 1: make dev-backend"
	@echo "  Terminal 2: make dev-frontend"

dev-backend:
	@echo "$(GREEN)🚀 Starting backend (port 8080)...$(NC)"
	cd api-server && dotnet run --project src/NaarNoor.API/NaarNoor.API.csproj

dev-frontend:
	@echo "$(GREEN)🎨 Starting frontend (port 5000)...$(NC)"
	cd naar-noor && npm run dev

test: test-backend test-frontend
	@echo "$(GREEN)✅ All tests passed!$(NC)"

test-backend:
	@echo "$(GREEN)🧪 Running backend tests...$(NC)"
	cd api-server && dotnet test

test-frontend:
	@echo "$(GREEN)🧪 Running frontend tests...$(NC)"
	cd naar-noor && npm test -- --run

lint:
	@echo "$(GREEN)🔍 Running linters...$(NC)"
	cd naar-noor && npm run lint

build: build-backend build-frontend
	@echo "$(GREEN)✅ Build complete!$(NC)"

build-backend:
	@echo "$(GREEN)🔨 Building backend...$(NC)"
	cd api-server && dotnet publish -c Release -o ./publish

build-frontend:
	@echo "$(GREEN)🔨 Building frontend...$(NC)"
	cd naar-noor && npm run build

docker-up:
	@echo "$(GREEN)🐳 Starting Docker services...$(NC)"
	docker-compose up -d
	@echo "$(GREEN)✅ Services started!$(NC)"

docker-down:
	@echo "$(YELLOW)⏹️  Stopping Docker services...$(NC)"
	docker-compose down

clean:
	@echo "$(YELLOW)🧹 Cleaning build artifacts...$(NC)"
	cd api-server && dotnet clean && rm -rf publish bin obj
	cd naar-noor && rm -rf dist node_modules/.cache
	@echo "$(GREEN)✅ Clean complete!$(NC)"

audit:
	@echo "$(GREEN)🔒 Running security audits...$(NC)"
	@echo "  NPM Audit:"
	cd naar-noor && npm audit --production || true
	@echo "  NuGet Audit:"
	cd api-server && dotnet list package --vulnerable || true
	@echo "$(GREEN)✅ Audit complete!$(NC)"

.DEFAULT_GOAL := help
