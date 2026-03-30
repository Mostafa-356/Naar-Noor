# Multi-stage build for Naar & Noor Restaurant

# Stage 1: Build the Angular application
FROM node:18-alpine AS build

WORKDIR /app

# Copy package files
COPY package*.json ./

# Install dependencies with proper npm configuration
RUN npm install --legacy-peer-deps --no-audit --prefer-offline

# Copy source code
COPY . .

# Build the application
RUN npm run build:prod

# Stage 2: Serve with Nginx
FROM nginx:alpine

# Copy custom nginx configuration
COPY nginx.conf /etc/nginx/nginx.conf

# Copy built application from build stage
COPY --from=build /app/dist/lost-yeti /usr/share/nginx/html

# Copy additional files
COPY --from=build /app/dist/lost-yeti/robots.txt /usr/share/nginx/html/
COPY --from=build /app/dist/lost-yeti/sitemap.xml /usr/share/nginx/html/

# Expose port 80
EXPOSE 80

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD wget --quiet --tries=1 --spider http://localhost/ || exit 1

# Start Nginx
CMD ["nginx", "-g", "daemon off;"]
