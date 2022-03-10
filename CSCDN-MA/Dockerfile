FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine

ENV \
    # Application guid
    APPLICATION_GUID="" \
    # Connection string
    CONNECTION_STRING="" \
    # CSCDN-MA version
    CSCDNMA_VERSION=1.0.0-build11 \
    # Disable the invariant mode (set in base image)
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

VOLUME [ "/data" ]

# Install cultures (same approach as Alpine SDK image)
RUN apk add --no-cache icu-libs -y

# Download CSCDN-MA binaries
RUN wget https://packages.czompi.cloud/products/czsoft/cscdn-ma/$CSCDNMA_VERSION.tar.gz -O cscdn-ma.tar.gz \
    && tar -ozxf cscdn-ma.tar.gz -C /bin \
    && rm cscdn-ma.tar.gz
WORKDIR /bin
EXPOSE 80
ENTRYPOINT ["dotnet", "CSCDN-MA.dll"]