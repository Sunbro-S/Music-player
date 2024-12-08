FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MusicManagerApi/MusicManagerApi.csproj", "MusicManagerApi/"]
RUN dotnet restore "MusicManagerApi/MusicManagerApi.csproj"
COPY . .
WORKDIR "/src/MusicManagerApi"
RUN dotnet build "MusicManagerApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MusicManagerApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MusicManagerApi.dll"]
