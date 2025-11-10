# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files
COPY ["Zeitung.Worker/Zeitung.Worker.csproj", "Zeitung.Worker/"]
COPY ["Zeitung.ServiceDefaults/Zeitung.ServiceDefaults.csproj", "Zeitung.ServiceDefaults/"]
RUN dotnet restore "Zeitung.Worker/Zeitung.Worker.csproj"

# Copy source code and build
COPY . .
WORKDIR "/src/Zeitung.Worker"
RUN dotnet build "Zeitung.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Zeitung.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Zeitung.Worker.dll"]
