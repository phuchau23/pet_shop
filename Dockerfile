FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/FoodBooking.Api/FoodBooking.Api.csproj", "src/FoodBooking.Api/"]
COPY ["src/FoodBooking.Application/FoodBooking.Application.csproj", "src/FoodBooking.Application/"]
COPY ["src/FoodBooking.Domain/FoodBooking.Domain.csproj", "src/FoodBooking.Domain/"]
COPY ["src/FoodBooking.Infrastructure/FoodBooking.Infrastructure.csproj", "src/FoodBooking.Infrastructure/"]

RUN dotnet restore "src/FoodBooking.Api/FoodBooking.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/FoodBooking.Api"
RUN dotnet build "FoodBooking.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FoodBooking.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FoodBooking.Api.dll"]