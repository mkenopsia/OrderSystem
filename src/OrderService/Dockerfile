FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Shared/Shared.csproj", "src/Shared/"]
COPY ["src/OrderService/OrderService.csproj", "src/OrderService/"]
RUN dotnet restore "src/OrderService/OrderService.csproj"
COPY . .
RUN dotnet build "src/OrderService/OrderService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/OrderService/OrderService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrderService.dll"]