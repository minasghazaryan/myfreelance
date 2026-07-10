# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY MyFreelance.sln ./
COPY src/MyFreelance.Domain/MyFreelance.Domain.csproj src/MyFreelance.Domain/
COPY src/MyFreelance.Application/MyFreelance.Application.csproj src/MyFreelance.Application/
COPY src/MyFreelance.Infrastructure/MyFreelance.Infrastructure.csproj src/MyFreelance.Infrastructure/
COPY src/MyFreelance.Web/MyFreelance.Web.csproj src/MyFreelance.Web/

RUN dotnet restore src/MyFreelance.Web/MyFreelance.Web.csproj

COPY src/ src/

RUN dotnet publish src/MyFreelance.Web/MyFreelance.Web.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/* \
    && mkdir -p /app/uploads \
    && chown -R app:app /app

USER app

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:8080/ || exit 1

ENTRYPOINT ["dotnet", "MyFreelance.Web.dll"]
