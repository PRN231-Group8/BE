##See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.
#
#FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
#USER app
#WORKDIR /app
#EXPOSE 8080
#EXPOSE 8081
#
#FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
#ARG BUILD_CONFIGURATION=Release
#WORKDIR /src
#COPY ["BE/BE.csproj", "BE/"]
#COPY ["PRN.ExploreNow.Data/PRN.ExploreNow.Data.csproj", "PRN.ExploreNow.Data/"]
#COPY ["PRN.ExploreNow.Domain/PRN.ExploreNow.Domain.csproj", "PRN.ExploreNow.Domain/"]
#COPY ["PRN.ExploreNow.Handlers/PRN.ExploreNow.Handlers.csproj", "PRN.ExploreNow.Handlers/"]
#COPY ["PRN.ExploreNow.Services/PRN.ExploreNow.Services.csproj", "PRN.ExploreNow.Services/"]
#RUN dotnet restore "./BE/BE.csproj"
#COPY . .
#WORKDIR "/src/BE"
#RUN dotnet build "./BE.csproj" -c $BUILD_CONFIGURATION -o /app/build
#
#FROM build AS publish
#ARG BUILD_CONFIGURATION=Release
#RUN dotnet publish "./BE.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
#
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "BE.dll"]

# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["BE.csproj", "BE/"]
COPY ["../PRN.ExploreNow.Data/PRN.ExploreNow.Data.csproj", "PRN.ExploreNow.Data/"]
COPY ["../PRN.ExploreNow.Domain/PRN.ExploreNow.Domain.csproj", "PRN.ExploreNow.Domain/"]
COPY ["../PRN.ExploreNow.Handlers/PRN.ExploreNow.Handlers.csproj", "PRN.ExploreNow.Handlers/"]
COPY ["../PRN.ExploreNow.Services/PRN.ExploreNow.Services.csproj", "PRN.ExploreNow.Services/"]
RUN dotnet restore "BE/BE.csproj"
COPY . .
WORKDIR "/src/BE"
RUN dotnet build "BE.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "BE.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BE.dll"]