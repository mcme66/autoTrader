# syntax=docker/dockerfile:1

FROM node:24-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

COPY . .
RUN npm run build

FROM nginx:1.31-alpine AS final

# nginx:alpine ships an unprivileged variant of its config; running as non-root means it
# cannot write the default pid/cache paths, so they are relocated under /tmp.
COPY nginx.conf /etc/nginx/nginx.conf
COPY --from=build /app/dist /usr/share/nginx/html

RUN touch /tmp/nginx.pid \
    && chown -R nginx:nginx /tmp/nginx.pid /var/cache/nginx /usr/share/nginx/html

USER nginx
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD wget --quiet --spider http://localhost:8080/healthz || exit 1

CMD ["nginx", "-g", "daemon off;"]
