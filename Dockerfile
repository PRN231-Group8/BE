#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["BE/PRN231.ExploreNow.API.csproj", "BE/"]
COPY ["ExploreNow.Validations/PRN231.ExploreNow.Validations.csproj", "ExploreNow.Validations/"]
COPY ["Services/PRN231.ExploreNow.Services.csproj", "Services/"]
COPY ["Data/PRN231.ExploreNow.Repositories.csproj", "Data/"]
COPY ["PRN231.ExploreNow.BusinessObject/PRN231.ExploreNow.BusinessObject.csproj", "PRN231.ExploreNow.BusinessObject/"]
RUN dotnet restore "./BE/PRN231.ExploreNow.API.csproj"
COPY . .
WORKDIR "/src/BE"
RUN dotnet build "./PRN231.ExploreNow.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./PRN231.ExploreNow.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER app
ENV ASPNETCORE_URLS="http://0.0.0.0:80"
ENTRYPOINT ["dotnet", "PRN231.ExploreNow.API.dll"]