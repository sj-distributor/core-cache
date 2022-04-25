FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["CoreCache/CoreCache.csproj", "CoreCache/"]
RUN dotnet restore "CoreCache/CoreCache.csproj"
COPY . .
WORKDIR "/src/CoreCache"
RUN dotnet build "CoreCache.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CoreCache.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CoreCache.dll"]
