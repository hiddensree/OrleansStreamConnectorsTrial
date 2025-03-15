# Use the official .NET SDK image to build the project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the project files
COPY . .

# Restore dependencies
RUN dotnet restore

# Build the project
RUN dotnet publish -c Release -o out

# Use the official .NET runtime image to run the project
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Set the entry point for the container
ENTRYPOINT ["dotnet", "LimOrleansServerOnlySimulator.dll"]