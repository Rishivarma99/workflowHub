# Staging / production image: Angular build + nginx (/api reverse proxy).
FROM node:22-alpine AS build
ARG BUILD_CONFIGURATION=production
WORKDIR /app

COPY frontend/package.json frontend/package-lock.json ./
RUN npm ci

COPY frontend/ .
RUN npm run build -- --configuration=${BUILD_CONFIGURATION}

FROM nginx:1.27-alpine

ENV API_PROXY_TARGET=http://host.docker.internal:8080

COPY docker/nginx/default.conf.template /etc/nginx/templates/default.conf.template
COPY --from=build /app/dist/workflow-hub/browser /usr/share/nginx/html

EXPOSE 80
