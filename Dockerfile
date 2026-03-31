# ═══════════════════════════════════════════════════════════════════════════════
# Dockerfile — Website QLPT (Multi-stage production build)
# ═══════════════════════════════════════════════════════════════════════════════

# ─── Base image (runtime) ─────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

# Non-root user for security hardening
RUN addgroup --system --gid 1001 appgroup && \
    adduser --system --uid 1001 --ingroup appgroup appuser

WORKDIR /app
EXPOSE 8080

# Create required directories with correct permissions
RUN mkdir -p /app/logs /app/wwwroot/uploads/rooms && \
    chown -R appuser:appgroup /app/logs /app/wwwroot/uploads

# ─── Build stage ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Website_QLPT.csproj", "./"]
RUN dotnet restore "Website_QLPT.csproj"

COPY . .
RUN dotnet build "Website_QLPT.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ─── Publish stage ────────────────────────────────────────────────────────────
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Website_QLPT.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false \
    /p:PublishSingleFile=false

# ─── Final production image ──────────────────────────────────────────────────
FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

# Switch to non-root user
USER appuser

# Health check — uses wget (available in base image, curl is NOT)
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health/live || exit 1

ENTRYPOINT ["dotnet", "Website_QLPT.dll"]
