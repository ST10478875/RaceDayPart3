# ---------------------------------------------------
# Stage 1: Runtime Base Image
# ---------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# ---------------------------------------------------
# SDK Image for Restoring & Building
# ---------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore NuGet packages
COPY ["RaceDayMVC/RaceDayMVC.csproj", "RaceDayMVC/"]
RUN dotnet restore "RaceDayMVC/RaceDayMVC.csproj"

# Copy all source code and compile the application in Release mode
COPY . .
WORKDIR "/src/RaceDayMVC"
RUN dotnet build "RaceDayMVC.csproj" -c Release -o /app/build

# ---------------------------------------------------
# Publish Binaries
# ---------------------------------------------------
FROM build AS publish
RUN dotnet publish "RaceDayMVC.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ---------------------------------------------------
# Final Production Image
# ---------------------------------------------------
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RaceDayMVC.dll"]