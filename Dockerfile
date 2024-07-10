FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY oil_tanks_webapi.csproj oil_tanks_webapi.csproj
RUN dotnet restore
COPY . .
WORKDIR /src
RUN dotnet build oil_tanks_webapi.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish oil_tanks_webapi.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT [ "dotnet", "oil_tanks_webapi.dll" ]