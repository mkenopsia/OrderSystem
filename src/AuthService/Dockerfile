FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /solution

COPY ["src/Shared/Shared.csproj", "src/Shared/"]
COPY ["src/AuthService/AuthService.csproj", "src/AuthService/"]

RUN dotnet restore "src/AuthService/AuthService.csproj"

COPY . .

RUN dotnet build "src/AuthService/AuthService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/AuthService/AuthService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AuthService.dll"]