# build stage
FROM node:23-slim AS build

WORKDIR /app

COPY ./package.json ./package-lock.json ./
RUN npm install

COPY . .
RUN npm run build

# production stage
FROM node:23-slim AS production

WORKDIR /app

COPY --from=build /app/dist ./dist

RUN npm i -g serve

EXPOSE 5173

CMD ["serve", "-s", "dist", "-l", "5173"]
