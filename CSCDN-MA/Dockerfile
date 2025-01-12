#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
EXPOSE 80
RUN apk add --no-cache icu-libs

ENV \
    # Connection string
    CONNECTION_STRING="" \
    # CSCDN-MA version
    CSCDNMA_VERSION=1.1.0-build12 \
    # Disable the invariant mode (set in base image)
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    # Colors in log
    ASPNETCORE_LOGGING__CONSOLE__DISABLECOLORS=false

VOLUME [ "/data" ]


FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
RUN apk add --no-cache icu-libs
WORKDIR /src
COPY ["CSCDN-MA/CSCDN-MA.csproj", "CSCDN-MA/"]
COPY ["nuget.config", "CSCDN-MA/nuget.config"]
RUN dotnet restore "CSCDN-MA/CSCDN-MA.csproj"
COPY . .
WORKDIR "/src/CSCDN-MA"
RUN dotnet build "CSCDN-MA.csproj" -c Beta -o /app/build

FROM build AS publish
RUN dotnet publish "CSCDN-MA.csproj" -c Beta -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Source: https://www.docker.com/blog/9-tips-for-containerizing-your-net-application/
HEALTHCHECK CMD curl --fail http://localhost/api || exit 1

ENTRYPOINT ["dotnet", "CSCDN-MA.dll"]