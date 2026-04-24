FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MichaelPage.API/MichaelPage.API.csproj", "MichaelPage.API/"]
COPY ["MichaelPage.Application/MichaelPage.Application.csproj", "MichaelPage.Application/"]
COPY ["MichaelPage.Core/MichaelPage.Core.csproj", "MichaelPage.Core/"]
COPY ["MichaelPage.Common/MichaelPage.Common.csproj", "MichaelPage.Common/"]
COPY ["MichaelPage.Infrastructure/MichaelPage.Infrastructure.csproj", "MichaelPage.Infrastructure/"]
RUN dotnet restore "MichaelPage.API/MichaelPage.API.csproj"
COPY . .
WORKDIR "/src/MichaelPage.API"
RUN dotnet build "./MichaelPage.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MichaelPage.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MichaelPage.API.dll"]
