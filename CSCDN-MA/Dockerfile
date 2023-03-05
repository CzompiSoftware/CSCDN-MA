FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env

WORKDIR /build

VOLUME [ "/data" ]

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Beta -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine
WORKDIR /app

ENV \
    # Application guid
    APPLICATION_GUID="" \
    # Connection string
    CONNECTION_STRING="" \
    # CSCDN-MA version
    CSCDNMA_VERSION=1.0.0-build11 \
    # Disable the invariant mode (set in base image)
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    # Colors in log
    ASPNETCORE_LOGGING__CONSOLE__DISABLECOLORS=false

VOLUME [ "/data" ]

RUN apk add --no-cache icu-libs

COPY --from=build-env /build/out .

EXPOSE 80
ENTRYPOINT dotnet "CSCDN-MA.dll" "--connectionString $CONNECTION_STRING --appGuid $APPLICATION_GUID"