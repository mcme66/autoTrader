# Build context is the repository root, so `backend/config/universe.json` can be copied
# alongside the published API.
# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

# Restore before copying sources so a source-only change does not invalidate the package
# cache. Central package management means the two props files are part of the graph.
COPY backend/Directory.Build.props backend/Directory.Packages.props backend/FinanceAnalysis.sln ./backend/
COPY backend/src/FinanceAnalysis.Domain/*.csproj ./backend/src/FinanceAnalysis.Domain/
COPY backend/src/FinanceAnalysis.Application/*.csproj ./backend/src/FinanceAnalysis.Application/
COPY backend/src/FinanceAnalysis.Infrastructure/*.csproj ./backend/src/FinanceAnalysis.Infrastructure/
COPY backend/src/FinanceAnalysis.Api/*.csproj ./backend/src/FinanceAnalysis.Api/
RUN dotnet restore backend/src/FinanceAnalysis.Api/FinanceAnalysis.Api.csproj

COPY backend/src/ ./backend/src/
RUN dotnet publish backend/src/FinanceAnalysis.Api/FinanceAnalysis.Api.csproj \
    --no-restore \
    --configuration Release \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app

# Kestrel listens on 8080 because the non-root user cannot bind a privileged port.
ENV ASPNETCORE_HTTP_PORTS=8080 \
    DOTNET_EnableDiagnostics=0

RUN adduser --system --uid 64198 --shell /sbin/nologin app \
    && addgroup --system --gid 64198 app \
    && adduser app app

COPY --from=build /app/publish ./
COPY backend/config/ ./config/

USER app
EXPOSE 8080

ENTRYPOINT ["dotnet", "FinanceAnalysis.Api.dll"]
