FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /build
COPY . .
RUN dotnet publish src/Nanoservice -c release -o out

FROM microsoft/dotnet:2.2-aspnetcore-runtime
WORKDIR /app
COPY --from=build /build/src/Nanoservice/out .
ENV ASPNETCORE_ENVIRONMENT docker
ENV ASPNETCORE_URLS http://*:5000
ENTRYPOINT dotnet Nanoservice.dll