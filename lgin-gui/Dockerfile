FROM node:latest AS build
WORKDIR /app
COPY . .
RUN npm install
RUN npm run build-prod

FROM nginx:latest
COPY --from=build /app/dist/gui /usr/share/nginx/html