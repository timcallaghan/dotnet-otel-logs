FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["WebHost/WebHost.csproj", "WebHost/"]
RUN dotnet restore "WebHost/WebHost.csproj"
COPY . .
WORKDIR "/src/WebHost"
RUN dotnet build "WebHost.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebHost.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebHost.dll"]
