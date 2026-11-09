FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["RaceDayPart2/RaceDayPart2.csproj", "RaceDayPart2/"]
RUN dotnet restore "RaceDayPart2/RaceDayPart2.csproj"

COPY . .
WORKDIR "/src/RaceDayPart2"
RUN dotnet publish "RaceDayPart2.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "RaceDayPart2.dll"]