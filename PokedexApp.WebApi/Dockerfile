# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy all project files for dependency resolution
COPY ["PokedexApp.WebApi/PokedexApp.WebApi.csproj", "PokedexApp.WebApi/"]
COPY ["PokedexApp.BasicInfo/PokedexApp.BasicInfo.csproj", "PokedexApp.BasicInfo/"]
COPY ["PokedexApp.Common/PokedexApp.Common.csproj", "PokedexApp.Common/"]
COPY ["PokedexApp.Translator/PokedexApp.Translator.csproj", "PokedexApp.Translator/"]

# Restore dependencies for the main web project (this will restore all referenced projects)
RUN dotnet restore "./PokedexApp.WebApi/PokedexApp.WebApi.csproj"

# Copy all source code
COPY . .

# Build the solution
WORKDIR "/src"
RUN dotnet build "PokedexApp.WebApi/PokedexApp.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR "/src"
RUN dotnet publish "PokedexApp.WebApi/PokedexApp.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PokedexApp.WebApi.dll"]