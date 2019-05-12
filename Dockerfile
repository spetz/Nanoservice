FROM mcr.microsoft.com/dotnet/core/sdk AS build
WORKDIR /build
COPY . .
RUN dotnet publish src/Nanoservice -c release -o out

FROM mcr.microsoft.com/dotnet/core/aspnet:2.2
WORKDIR /app
COPY --from=build /build/src/Nanoservice/out .
ENV ASPNETCORE_ENVIRONMENT docker
ENV ASPNETCORE_URLS http://*:5000
ENTRYPOINT dotnet Nanoservice.dll