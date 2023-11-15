FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/GymApp/GymApp.csproj", "src/GymApp/"]
RUN dotnet restore "src/GymApp/GymApp.csproj"
COPY . .
WORKDIR "/src/src/GymApp"
RUN dotnet build "GymApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GymApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GymApp.dll"]
