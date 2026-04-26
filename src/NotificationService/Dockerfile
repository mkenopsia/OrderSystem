FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Shared/Shared.csproj", "Shared/"]
COPY ["NotificationService/NotificationService.csproj", "NotificationService/"]
RUN dotnet restore "NotificationService/NotificationService.csproj"
COPY . .
WORKDIR "/src/NotificationService"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NotificationService.dll"]