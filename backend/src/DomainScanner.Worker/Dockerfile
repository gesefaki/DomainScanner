FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["backend/src/DomainScanner.Worker/DomainScanner.Worker.csproj", "backend/src/DomainScanner.Worker/"]
COPY ["backend/src/DomainScanner.Application/DomainScanner.Application.csproj", "backend/src/DomainScanner.Application/"]
COPY ["backend/src/DomainScanner.Contracts/DomainScanner.Contracts.csproj", "backend/src/DomainScanner.Contracts/"]
COPY ["backend/src/DomainScanner.Domain/DomainScanner.Domain.csproj", "backend/src/DomainScanner.Domain/"]
COPY ["backend/src/DomainScanner.Infrastructure/DomainScanner.Infrastructure.csproj", "backend/src/DomainScanner.Infrastructure/"]
COPY ["backend/src/DomainScanner.Shared/DomainScanner.Shared.csproj", "backend/src/DomainScanner.Shared/"]
RUN dotnet restore "backend/src/DomainScanner.Worker/DomainScanner.Worker.csproj"
COPY . .
WORKDIR "/src/backend/src/DomainScanner.Worker"
RUN dotnet build "./DomainScanner.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DomainScanner.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DomainScanner.Worker.dll"]
