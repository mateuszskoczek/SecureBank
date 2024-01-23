FROM nginx:alpine
ARG KEY_PASSWORD=default

COPY ./Nginx/nginx.conf /etc/nginx/nginx.conf
COPY ./Nginx/localhost.crt /etc/ssl/certs/localhost.crt
COPY ./Nginx/localhost.key /etc/ssl/private/localhost.key