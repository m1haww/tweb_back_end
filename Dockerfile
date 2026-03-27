# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY "Groz Backend.sln" .
COPY global.json .
COPY Api/Api.csproj Api/
COPY BusinessLogic/BusinessLogic.csproj BusinessLogic/
COPY Domain/Domain.csproj Domain/

# Restore
RUN dotnet restore Api/Api.csproj

# Copy remaining source and publish
COPY . .
RUN dotnet publish Api/Api.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# PORT is set by Railway at runtime; Program.cs binds to 0.0.0.0:PORT
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Api.dll"]
