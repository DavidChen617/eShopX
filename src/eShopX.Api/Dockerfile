# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY eShopx.slnx ./
COPY global.json ./
COPY src/ApplicationCore/ApplicationCore.csproj src/ApplicationCore/
COPY src/Infrastructure/Infrastructure.csproj src/Infrastructure/
COPY src/eShopX.Common/eShopX.Common.csproj src/eShopX.Common/
COPY src/eShopX.Api/eShopX.Api.csproj src/eShopX.Api/

RUN dotnet restore src/eShopX.Api/eShopX.Api.csproj

COPY src/ ./src/

RUN dotnet publish src/eShopX.Api/eShopX.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://0.0.0.0:5177
EXPOSE 5177

COPY --from=build /app/publish ./

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

ENTRYPOINT ["dotnet", "eShopX.Api.dll"]
