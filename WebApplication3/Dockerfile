# Base stage - Runtime environment
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
EXPOSE 5134

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files first for efficient layer caching
COPY WebApplication3/WebApplication3.csproj WebApplication3/
WORKDIR /src/WebApplication3
RUN dotnet restore

# Copy the full source code (excluding tests)
COPY WebApplication3/. .

# **Build the main project ONLY (skip test projects)**
RUN dotnet build -c $BUILD_CONFIGURATION -o /app/build

# **Run unit tests (Optional but recommended)**
FROM build AS test
WORKDIR /src/WebApplication3
COPY TestProject1/. ./TestProject1
RUN dotnet test ./TestProject1/TestProject1.csproj --no-build --verbosity normal

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final runtime stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebApplication3.dll"]
