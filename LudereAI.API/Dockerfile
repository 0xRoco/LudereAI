FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["LudereAI.API/LudereAI.API.csproj", "LudereAI.API/"]
COPY ["LudereAI.Application/LudereAI.Application.csproj", "LudereAI.Application/"]
COPY ["LudereAI.Domain/LudereAI.Domain.csproj", "LudereAI.Domain/"]
COPY ["LudereAI.Shared/LudereAI.Shared.csproj", "LudereAI.Shared/"]
COPY ["LudereAI.Infrastructure/LudereAI.Infrastructure.csproj", "LudereAI.Infrastructure/"]
RUN dotnet restore "LudereAI.API/LudereAI.API.csproj"
COPY . .
WORKDIR "/src/LudereAI.API"
RUN dotnet build "LudereAI.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "LudereAI.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LudereAI.API.dll"]
