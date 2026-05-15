FROM node:20-alpine AS module-builder

WORKDIR /modules

RUN apk add --no-cache git

COPY modules/package*.json ./
RUN npm install

COPY modules/tsconfig.json ./
COPY modules/src ./src
RUN npm run build

FROM registry.heroiclabs.com/heroiclabs/nakama:3.38.0

COPY --from=module-builder /modules/build/*.js /nakama/data/modules/build/
COPY config/local.yml /nakama/data/local.yml
