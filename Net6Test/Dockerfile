FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Net6Test/Net6Test.csproj", "Net6Test/"]
RUN dotnet restore "Net6Test/Net6Test.csproj"
COPY . .
WORKDIR "/src/Net6Test"
RUN dotnet build "Net6Test.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Net6Test.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Net6Test.dll"]
