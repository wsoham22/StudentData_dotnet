# Use official .NET SDK as the build environment
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory in the container
WORKDIR /app

# Copy the project file and restore dependencies
COPY WebApplication3/WebApplication3.csproj WebApplication3/
WORKDIR /app/WebApplication3
RUN dotnet restore

# Copy the rest of the application source code
COPY WebApplication3/ ./

# Build the application
RUN dotnet publish -c Release -o /publish

# Use a lightweight .NET runtime for production
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy the build output to the runtime container
COPY --from=build /publish .
EXPOSE 5134
# Set the entry point
ENTRYPOINT ["dotnet", "WebApplication3.dll"]
