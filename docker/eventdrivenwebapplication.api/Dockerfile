FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
EXPOSE 1433
EXPOSE 5672

# Install OpenSSL
RUN apt-get update && apt-get install -y openssl

# Generate certificates only if they don't exist, adding SAN extension
RUN mkdir -p /app/certs && \
    if [ ! -f /app/certs/aspnetapp.pfx ]; then \
        openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout /app/certs/aspnetapp.key -out /app/certs/aspnetapp.crt -subj "/CN=localhost" -addext "subjectAltName=DNS:localhost,IP:127.0.0.1" && \
        openssl pkcs12 -export -out /app/certs/aspnetapp.pfx -inkey /app/certs/aspnetapp.key -in /app/certs/aspnetapp.crt -password pass:your_password && \
        chmod 644 /app/certs/aspnetapp.pfx; \
    fi

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /

COPY ["src/EventDrivenWebApplication.API/EventDrivenWebApplication.API.csproj", "EventDrivenWebApplication.API/"]
COPY ["src/EventDrivenWebApplication.Domain/EventDrivenWebApplication.Domain.csproj", "EventDrivenWebApplication.Domain/"]
COPY ["src/EventDrivenWebApplication.Infrastructure/EventDrivenWebApplication.Infrastructure.csproj", "EventDrivenWebApplication.Infrastructure/"]
RUN dotnet restore "EventDrivenWebApplication.API/EventDrivenWebApplication.API.csproj"

# Copy the remaining source code
COPY . .

# Build the project
WORKDIR "/src/EventDrivenWebApplication.API"
RUN dotnet build "EventDrivenWebApplication.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage for publishing the app
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "EventDrivenWebApplication.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage for creating the runtime image
FROM base AS final
WORKDIR /app

# Copy the published output from the publish stage
COPY --from=publish /app/publish .

# Ensure the application runs as the 'app' user
RUN chown -R app:app /app
USER app

ENTRYPOINT ["dotnet", "EventDrivenWebApplication.API.dll"]