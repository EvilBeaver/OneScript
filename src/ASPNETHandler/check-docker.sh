docker build -t oscript/http-service .

docker run --rm -it -p 8080:80  oscript/http-service
