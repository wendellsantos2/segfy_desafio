FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy sln and csproj files
COPY ["Sinistros.sln", "./"]
COPY ["Sinistros.API/Sinistros.API.csproj", "Sinistros.API/"]
COPY ["Sinistros.Application/Sinistros.Application.csproj", "Sinistros.Application/"]
COPY ["Sinistros.Domain/Sinistros.Domain.csproj", "Sinistros.Domain/"]
COPY ["Sinistros.Infrastructure/Sinistros.Infrastructure.csproj", "Sinistros.Infrastructure/"]
COPY ["Sinistros.Tests/Sinistros.Tests.csproj", "Sinistros.Tests/"]

# Restore packages
RUN dotnet restore

# Copy all source code
COPY . .

# Build and Publish
WORKDIR "/app/Sinistros.API"
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Generate runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=5113
EXPOSE 5113

ENTRYPOINT ["dotnet", "Sinistros.API.dll"]
