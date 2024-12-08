FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["identityApi/identityApi.csproj", "identityApi/"]
RUN dotnet restore "identityApi/identityApi.csproj"
COPY . .
WORKDIR "/src/identityApi"
RUN dotnet build "identityApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "identityApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "identityApi.dll"]
