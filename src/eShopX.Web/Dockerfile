FROM node:20-alpine AS build
WORKDIR /app

ARG API_BASE_URL=https://eshop-x.com/api
ENV API_BASE_URL=${API_BASE_URL}

COPY package.json pnpm-lock.yaml ./
RUN corepack enable && pnpm install --frozen-lockfile

COPY . .
RUN sed -i "s|__API_BASE_URL__|${API_BASE_URL}|g" src/environments/environment.prod.ts
RUN pnpm run build

FROM nginx:1.27-alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/e-shop-x.web/browser /usr/share/nginx/html
