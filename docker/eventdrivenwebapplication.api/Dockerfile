# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /

COPY ["src/EventDrivenWebApplication.API/EventDrivenWebApplication.API.csproj", "EventDrivenWebApplication.API/"]
COPY ["src/EventDrivenWebApplication.Domain/EventDrivenWebApplication.Domain.csproj", "EventDrivenWebApplication.Domain/"]
COPY ["src/EventDrivenWebApplication.Infrastructure/EventDrivenWebApplication.Infrastructure.csproj", "EventDrivenWebApplication.Infrastructure/"]
RUN dotnet restore "./src/EventDrivenWebApplication.API/EventDrivenWebApplication.API.csproj"
COPY . .
WORKDIR "/src/EventDrivenWebApplication.API"
RUN dotnet build "EventDrivenWebApplication.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "EventDrivenWebApplication.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage for creating the runtime image
FROM base AS final
WORKDIR /app
# Copy the published output from the publish stage
COPY --from=publish /app/publish .
# Ensure the application runs as the 'app' user and set proper permissions
RUN chown -R app:app /app
# Define the entry point for the application
USER app
ENTRYPOINT ["dotnet", "EventDrivenWebApplication.API.dll"]