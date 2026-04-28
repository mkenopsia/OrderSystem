FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Shared/Shared.csproj", "src/Shared/"]
COPY ["src/InventoryService/InventoryService.csproj", "src/InventoryService/"]
RUN dotnet restore "src/InventoryService/InventoryService.csproj"
COPY . .

RUN dotnet build "src/InventoryService/InventoryService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/InventoryService/InventoryService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InventoryService.dll"]