FROM node:24 AS build

WORKDIR /app

COPY package*.json ./

RUN npm ci

COPY . .

RUN npm run build


FROM nginx:alpine

ARG NGINX_CONF=nginx.dev.conf

COPY --from=build /app/build /usr/share/nginx/html

COPY ${NGINX_CONF} /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]