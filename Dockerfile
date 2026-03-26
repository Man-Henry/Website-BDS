# Base image cho runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

# Chạy với non-root user để tăng bảo mật
RUN addgroup --system --gid 1001 appgroup && \
    adduser --system --uid 1001 --ingroup appgroup appuser

WORKDIR /app
EXPOSE 80
EXPOSE 443

# Tạo thư mục logs với quyền của appuser
RUN mkdir -p /app/logs && chown appuser:appgroup /app/logs

# ─── Build stage ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Website_QLPT.csproj", "./"]
RUN dotnet restore "Website_QLPT.csproj"

COPY . .
RUN dotnet build "Website_QLPT.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ─── Publish stage ────────────────────────────────────────────────────────
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Website_QLPT.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false \
    /p:PublishSingleFile=false

# ─── Final production image ───────────────────────────────────────────────
FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

# Chuyển sang non-root user trước khi run
USER appuser

# Health check tích hợp Docker
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:80/health/live || exit 1

ENTRYPOINT ["dotnet", "Website_QLPT.dll"]
