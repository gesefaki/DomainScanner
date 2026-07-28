FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER root
RUN mkdir -p /var/lib/domainscanner/keys /https \
    && chown -R $APP_UID:$APP_UID /var/lib/domainscanner/keys /https
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["backend/src/DomainScanner.Api/DomainScanner.Api.csproj", "backend/src/DomainScanner.Api/"]
COPY ["backend/src/DomainScanner.Application/DomainScanner.Application.csproj", "backend/src/DomainScanner.Application/"]
COPY ["backend/src/DomainScanner.Domain/DomainScanner.Domain.csproj", "backend/src/DomainScanner.Domain/"]
COPY ["backend/src/DomainScanner.Infrastructure/DomainScanner.Infrastructure.csproj", "backend/src/DomainScanner.Infrastructure/"]
COPY ["backend/src/DomainScanner.Contracts/DomainScanner.Contracts.csproj", "backend/src/DomainScanner.Contracts/"]
COPY ["backend/src/DomainScanner.Shared/DomainScanner.Shared.csproj", "backend/src/DomainScanner.Shared/"]
COPY ["backend/src/DomainScanner.Worker/DomainScanner.Worker.csproj", "backend/src/DomainScanner.Worker/"]
RUN dotnet restore "backend/src/DomainScanner.Api/DomainScanner.Api.csproj"
COPY . .
WORKDIR "/src/backend/src/DomainScanner.Api"
RUN dotnet build "./DomainScanner.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DomainScanner.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DomainScanner.Api.dll"]
