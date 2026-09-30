# =========================================================
# BASE
# =========================================================

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

WORKDIR /app

EXPOSE 8080
EXPOSE 8081

USER root

# Katalog na uploady
RUN mkdir -p /app/App_Data/uploads \
    && chown -R $APP_UID:$APP_UID /app/App_Data

USER $APP_UID


# =========================================================
# BUILD
# =========================================================

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

ARG BUILD_CONFIGURATION=Release

WORKDIR /src

COPY ["Lumo.Api/Lumo.Api.csproj", "Lumo.Api/"]

RUN dotnet restore "./Lumo.Api/Lumo.Api.csproj"

COPY . .

WORKDIR "/src/Lumo.Api"

RUN dotnet build "./Lumo.Api.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/build


# =========================================================
# PUBLISH
# =========================================================

FROM build AS publish

ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "./Lumo.Api.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false


# =========================================================
# FINAL
# =========================================================

FROM base AS final

USER root

WORKDIR /app

COPY --from=publish /app/publish .

# ---------------------------------------------------------
# Permissions
# ---------------------------------------------------------

RUN chown -R $APP_UID:$APP_UID /app

USER $APP_UID

# =========================================================
# START
# =========================================================

ENTRYPOINT ["dotnet", "Lumo.Api.dll"]