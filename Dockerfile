# Build context is the repository root, so the whole solution is restorable.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy manifests first so `restore` is cached until a dependency actually changes.
# global.json is deliberately not copied: it pins the SDK for developer machines,
# and pinning it here would break the build whenever the base image ships an
# older feature band than 10.0.401. The image tag is the pin that matters here.
COPY src/SatinRoad.Domain/SatinRoad.Domain.csproj                 src/SatinRoad.Domain/
COPY src/SatinRoad.Infrastructure/SatinRoad.Infrastructure.csproj src/SatinRoad.Infrastructure/
COPY src/SatinRoad.Api/SatinRoad.Api.csproj                       src/SatinRoad.Api/
RUN dotnet restore src/SatinRoad.Api/SatinRoad.Api.csproj

COPY src/ src/
RUN dotnet publish src/SatinRoad.Api/SatinRoad.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .

# The SQLite file lives on a volume, so data survives a rebuild.
RUN mkdir -p /app/data && chown $APP_UID /app/data
USER $APP_UID

EXPOSE 8080
ENTRYPOINT ["dotnet", "SatinRoad.Api.dll"]