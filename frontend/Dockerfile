# Build stage
FROM node:20-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

COPY . .

# Production API URL (browser must reach the public backend, not Docker internal hostnames)
ARG API_URL=http://localhost:5150/api
RUN sed -i "s|http://localhost:5150/api|${API_URL}|g" src/app/environments/environment.ts \
    && npm run build

# Run stage
FROM nginx:1.27-alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf.template
COPY docker-entrypoint.sh /docker-entrypoint.sh
RUN chmod +x /docker-entrypoint.sh

COPY --from=build /app/dist/frontend/browser /usr/share/nginx/html

EXPOSE 8080
ENTRYPOINT ["/docker-entrypoint.sh"]
